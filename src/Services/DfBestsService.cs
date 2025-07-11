using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using LimeKuma;
using SixLabors.ImageSharp;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfDataAsync(
        uint qq, int frame = 200502, int plate = 101, int icon = 101)
    {
        DfResourceClient df = new();
        Player player = await df.GetPlayerAsync(qq);

        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;
        user.IconId = icon;

        List<CommonRecord> bestEver = player.Bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();
        int everTotal = bestEver.Sum(x => x.DXRating);

        List<CommonRecord> bestCurrent = player.Bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();
        int currentTotal = bestCurrent.Sum(x => x.DXRating);

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    public override async Task GetFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal);

        await bestsImage.WriteToResponseAsync(responseStream);
    }

    public override async Task GetAnimeFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal,
            BestsDrawer.BackgroundAnimationPath);

        await bestsImage.WriteToResponseAsync(responseStream, true);
    }
}