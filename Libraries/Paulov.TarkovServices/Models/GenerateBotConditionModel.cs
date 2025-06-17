namespace Paulov.TarkovServices.Models
{
    public class GenerateBotConditionModel
    {
        public string Role;

        public int Limit;

        public string Difficulty;

        public GenerateBotConditionModel(int count, string roleType, string difficulty)
        {
            Limit = count;
            Difficulty = difficulty;
            Role = roleType;
        }

        public static GenerateBotConditionModel CopyFromWaveInfo(WaveInfoClass waveInfoClass)
        {
            return new GenerateBotConditionModel(waveInfoClass.Limit, waveInfoClass.Role.ToString(), waveInfoClass.Difficulty.ToString());
        }

        public WaveInfoClass ToWaveInfoClass()
        {
            return new WaveInfoClass(
                Limit,
                Enum.Parse<EFT.WildSpawnType>(Role, true),
                Enum.Parse<BotDifficulty>(Difficulty, true)
            );
        }
    }
}
