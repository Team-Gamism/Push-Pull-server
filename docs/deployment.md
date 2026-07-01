# 배포 가이드

Push & Pull 서버의 환경 분리, 배포 파이프라인, 클라이언트 연동 정책을 정리한다.

## 환경 구성

같은 코드/이미지를 `ASPNETCORE_ENVIRONMENT` 값만 다르게 주어 환경을 구분한다. 서버 인스턴스는 환경별로 분리한다.

| 환경 | `ASPNETCORE_ENVIRONMENT` | 인스턴스 | 접속 대상 | Swagger |
|---|---|---|---|---|
| 로컬 개발 | `Development` | 로컬 실행 | 백엔드 개발자 PC | 노출 |
| 스테이지 | `Staging` | `pushandpull-stage` | 클라이언트 개발자 | 노출 |
| 운영 | `Production` | `pushandpull-prod` | 실제 유저 | **차단** |

stage와 prod는 **DB / Redis / 시크릿을 반드시 분리**한다. stage에서의 테스트 데이터가 운영 유저 데이터에 영향을 주지 않도록 하기 위함이다.

### DB 전략

| 환경 | PostgreSQL | 비고 |
|---|---|---|
| stage | compose 내 컨테이너(`pushandpull-stage-db`) | 테스트 데이터, 날아가도 무방 |
| prod | **별도 DB 인스턴스** | 다른 학교 서버 인스턴스에서 `compose.db.yaml`로 단독 운영. 앱은 `DB_CONNECTION_STRING`으로 원격 연결 |

### prod DB 인스턴스 (`compose.db.yaml`)

prod의 Postgres는 앱과 분리된 별도 학교 서버 인스턴스에서 돌린다. 학교 SVC 포워딩(내부 10000 → 외부 `ssh.gsmsv.site:23143`)에 맞춰 호스트 포트 10000에 바인딩한다.

```bash
# DB 인스턴스에서 (최초 1회)
docker volume create pushandpull-prod-postgres-data
printf 'POSTGRES_DB=%s\nPOSTGRES_USER=%s\nPOSTGRES_PASSWORD=%s\n' '<db>' '<user>' '<강력한_비밀번호>' > .env
docker compose -f compose.db.yaml --env-file .env up -d
```

prod 앱의 `DB_CONNECTION_STRING`은 이 인스턴스를 가리킨다:

```
Host=ssh.gsmsv.site;Port=23143;Database=<db>;Username=<user>;Password=<비밀번호>
```

> ⚠️ DB가 공개 포워딩으로 노출되므로 **강력한 비밀번호 필수**. 포워딩 포트는 0.0.0.0/0으로 열리니 IP 제한이 어렵다. 가능하면 SSH 터널 등 추가 보호를 검토한다.

Redis는 stage/prod 모두 compose 내 컨테이너로 띄우고 내부 네트워크로만 접근한다(고정 주소 `pushandpull-redis:6379`, 시크릿 불필요).

stage의 Postgres는 **external 전용 볼륨**(`pushandpull-stage-postgres-data`)에 데이터를 둔다. compose가 관리하는 볼륨과 달리 `docker compose down -v`나 프로젝트 삭제로 데이터가 날아가지 않는다. external 볼륨은 미리 존재해야 하므로 stage CD가 배포 전 멱등하게 생성한다:

```bash
docker volume create pushandpull-stage-postgres-data
```

(호스트 경로로 직접 관리하려면 `-v /data/postgres:/var/lib/postgresql/data` 같은 bind mount로 대체할 수도 있다.)

> ⚠️ stage 서버의 연결 문자열은 `compose.stage.yaml`에서 `STAGE_POSTGRES_PASSWORD`로 조립되며 값은 싱글쿼트로 인용된다(`Password='...'`). 덕분에 `;`·`=`·공백은 안전하지만, **비밀번호에 싱글쿼트(`'`)는 넣지 말 것**. Npgsql 연결 문자열 파서가 인용을 조기에 닫아 인증에 실패한다(굳이 쓰려면 `''`로 이스케이프). prod는 `DB_CONNECTION_STRING`을 통째로 주입하므로 이 제약이 없다.

## Swagger 노출 정책

`Program.cs`에서 환경에 따라 게이팅한다.

```csharp
options.Swagger.Enabled = !builder.Environment.IsProduction();
```

- `Production` → 차단
- `Staging`, `Development` → 노출 (`/swagger`, `/swagger/v1/swagger.json`)

Swagger는 `Gamism.SDK.Extensions.AspNetCore`가 내부적으로 Swashbuckle을 구성한다. SDK 자체에는 환경 분기가 없으므로 위 `Enabled` 플래그로 제어한다.

## 브랜치 → 서버 매핑

| 브랜치 | 워크플로우 | 동작 |
|---|---|---|
| `develop` push | `pushandpull-stage-cd.yml` | stage 서버 배포 (`:stage` 이미지) |
| `main` push | `pushandpull-prod-cd.yml` | 버전 태그/릴리스 생성 후 prod 서버 배포 (`:latest` 이미지) |

CI(빌드·테스트)는 `pushandpull-stage-ci.yml`(develop), `pushandpull-prod-ci.yml`이 담당한다.

흐름:

```
develop 머지 → stage 배포 → 클라 개발자가 개발 빌드로 검증
main 머지    → prod 배포   → 출시 빌드가 실제 유저에게 서비스
```

## Compose 구조

배포용 환경은 **부모(base) + 환경별 override** 패턴으로 구성한다. 공통 정의는 `compose.yaml`에 두고, prod/stage는 차이(이미지 태그·포트·환경값·컨테이너명)만 override 파일에 담는다.

| 파일 | 역할 |
|---|---|
| `deploy/compose.yaml` | 부모 — redis/server 공통 정의, 공통 env, 네트워크/볼륨 (**DB 미포함**) |
| `deploy/compose.prod.yaml` | prod override — `:latest`, 포트 80, `Production`, 외부 관리형 DB connstring |
| `deploy/compose.stage.yaml` | stage override — `:stage`, 포트 80, `Staging`, **DB 컨테이너 추가** |

DB는 환경마다 위치가 달라 base에 두지 않는다. prod는 connstring을 외부에서 주입하고, stage는 override에서 postgres 컨테이너를 직접 올린다.

실행은 두 파일을 합쳐서 한다:

```bash
docker compose -f compose.yaml -f compose.prod.yaml --env-file .env up -d   # prod
docker compose -f compose.yaml -f compose.stage.yaml --env-file .env up -d  # stage
```

| 환경 | 이미지 태그 | 호스트 포트 | 외부 접속 |
|---|---|---|---|
| stage | `seanyee1227/pushandpull-server:stage` | 80 | `ssh.gsmsv.site:25144` |
| prod | `seanyee1227/pushandpull-server:latest` | 80 | `ssh.gsmsv.site:25139` |

> prod는 학교 서버(GSM) 포트포워딩(내부 80 → 외부 25139)에 맞춰 호스트 포트를 80으로 둔다. 호스트 포트는 반드시 외부로 포워딩되는 내부 포트와 일치해야 외부 접속이 된다.

prod/stage 모두 `deploy/prod.dockerfile`로 빌드한다.

## 필요한 GitHub Secrets

### 공통
- `DOCKER_USERNAME`, `DOCKER_PASSWORD`
- `DISCORD_WEBHOOK`
- `STEAM_API_KEY`, `STEAM_APP_ID` — prod·stage 공용

### prod (`pushandpull-prod-cd.yml`)
- `SSH_HOST`, `SSH_USERNAME`, `SSH_PORT`, `SSH_PRIVATE_KEY`
- `SSH_FINGERPRINT` — 서버 호스트 키 SHA256 지문 (MITM 방지)
- `DB_CONNECTION_STRING` — 외부 관리형 Postgres 연결 문자열

### stage (`pushandpull-stage-cd.yml`)
- `STAGE_SSH_HOST`, `STAGE_SSH_USERNAME`, `STAGE_SSH_PORT`, `STAGE_SSH_PRIVATE_KEY`
- `STAGE_SSH_FINGERPRINT` — 서버 호스트 키 SHA256 지문 (MITM 방지)
- `STAGE_POSTGRES_DB`, `STAGE_POSTGRES_USER`, `STAGE_POSTGRES_PASSWORD` — in-compose DB 컨테이너 자격증명

> Steam 키(`STEAM_API_KEY`, `STEAM_APP_ID`)는 prod·stage 공용이라 공통 섹션에서 한 번만 등록한다.

> Redis는 내부 컨테이너(고정 주소)라 시크릿이 없다.

호스트 키 지문(`*_SSH_FINGERPRINT`)은 배포 대상 서버에서 아래로 얻는다. appleboy/scp-action·ssh-action(Go `x/crypto/ssh`)은 호스트 키 협상 시 ECDSA를 ED25519보다 우선하므로 **ECDSA 키 지문**을 등록해야 한다. ED25519 지문을 넣으면 `ssh: handshake failed: host key fingerprint mismatch`로 실패한다.

```bash
ssh-keyscan -p <port> -t ecdsa <host> | ssh-keygen -lf - | awk '{print $2}'
# 예: SHA256:abc123...  (이 값 전체를 시크릿에 등록)
```

> stage 배포를 활성화하려면 위 `STAGE_*` 시크릿을 먼저 등록해야 한다.

## 클라이언트(Unity) 서버 URL 분리

클라이언트는 빌드별로 접속 서버 URL을 **Scripting Define Symbols(`#if`) 방식**으로 분기한다. 출시 빌드에 stage 서버 정보가 컴파일되지 않아 안전하다.

```csharp
public static class ServerConfig
{
#if STAGE
    public const string BaseUrl = "https://stage.yourgame.com";   // stage 서버
#elif PRODUCTION
    public const string BaseUrl = "https://api.yourgame.com";      // prod 서버
#else // 에디터/로컬
    public const string BaseUrl = "http://localhost:8081";
#endif
}
```

- 개발/내부 테스트 빌드 → `STAGE` 심볼 → stage 서버
- 출시 빌드 → `PRODUCTION` 심볼 → prod 서버

> 백엔드 레포에서는 stage/prod 서버를 띄워 두기만 하면 되고, 어느 서버에 붙을지는 클라이언트 빌드 설정이 결정한다.
