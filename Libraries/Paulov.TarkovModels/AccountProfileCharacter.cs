namespace Paulov.TarkovModels
{
    /// <summary>
    /// TODO: Using this would be the preferred way, rather than reinventing the wheel
    /// The inherited class uses model descriptors (just like we did below!)
    /// We need GClass2030 to be Remapped!
    /// </summary>
    public class AccountProfileCharacter : CompleteProfileDescriptorClass
    {
        public AccountProfileCharacter() : base()
        {
        }

        public EFT.Profile GetProfile()
        {
            return new EFT.Profile(this);
        }

        public override string ToString()
        {
            if (Info != null)
            {
                return $"{Info.Nickname}:{Id}:{AccountId}:{Info.Settings.Role}";
            }

            return base.ToString() ?? "";
        }
    }

}
