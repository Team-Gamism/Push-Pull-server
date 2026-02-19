using Server.Application.Dto;
using Server.Application.Port.Input;
using Server.Application.Port.Output.Persistence;

namespace Server.Application.UseCase.Room;

public class GetAllRoomUseCase : IGetAllRoomUseCase
{
    private readonly IRoomRepository _roomRepository;

    public GetAllRoomUseCase(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }
    
    public async Task<GetAllRoomUseCaseResult> ExecuteAsync(CancellationToken ct = default)
    {
        var rooms = await _roomRepository.GetAllAsync(ct);
        
        var summaries = rooms
            .Select(room => new RoomSummary(
                room.RoomCode,
                room.RoomName,
                room.CurrentPlayers,
                room.IsPrivate
            ))
            .ToList();
        
        return new GetAllRoomUseCaseResult(summaries);
    }
}