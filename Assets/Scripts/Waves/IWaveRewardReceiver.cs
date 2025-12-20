namespace Havengard.Waves
{
    public interface IWaveRewardReceiver
    {
        void GrantWaveRewards(int gold, int exp, int celestium);
    }
}
