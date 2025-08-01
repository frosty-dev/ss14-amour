namespace Content.Shared.Alert
{
    /// <summary>
    /// Every kind of alert. Corresponds to alertType field in alert prototypes defined in YML
    /// NOTE: Using byte for a compact encoding when sending this in messages, can upgrade
    /// to ushort
    /// </summary>
    public enum AlertType : byte
    {
        Error,
        LowOxygen,
        LowNitrogen,
        LowPressure,
        HighPressure,
        Fire,
        Cold,
        Hot,
        Weightless,
        Stun,
        Handcuffed,
        Ensnared,
        Buckled,
        HumanCrit,
        HumanDead,
        HumanHealth,
        Bleeding,
        BorgBattery,
        BorgBatteryNone,
        //WD start
        Insane,
        VeryVeryBad,
        VeryBad,
        Bad,
        NotGreat,
        Neutral,
        Great,
        Good,
        VeryGood,
        VeryVeryGood,
        MoodDead,
        Arousal, //AMOUR
        CultBuffed,
        BloodSpells,
        Knockdown,
        RecentlyBlocked,
        //WD end
        PilotingShuttle,
        Peckish,
        Starving,
        Thirsty,
        Parched,
        Stamina,
        Pulled,
        Pulling,
        Magboots,
        Internals,
        Toxins,
        Muted,
        VowOfSilence,
        VowBroken,
        Essence,
        Chemicals,
        Corporeal,
        Bleed,
        Pacified,
        Debug1,
        Debug2,
        Debug3,
        Debug4,
        Debug5,
        Debug6,
        SuitPower,
        BorgHealth,
        BorgCrit,
        BorgDead,
        Deflecting,
        Offer // WD EDIT
    }

}
