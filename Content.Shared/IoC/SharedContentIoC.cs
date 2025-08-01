﻿using Content.Shared.Changeling;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Localizations;
using Content.Shared._White.Cult.Systems;

namespace Content.Shared.IoC
{
    public static class SharedContentIoC
    {
        public static void Register()
        {
            IoCManager.Register<MarkingManager, MarkingManager>();
            IoCManager.Register<ContentLocalizationManager, ContentLocalizationManager>();
            IoCManager.Register<ChangelingNameGenerator, ChangelingNameGenerator>();
            // WD EDIT
            IoCManager.Register<CultistWordGeneratorManager, CultistWordGeneratorManager>();
            // WD EDIT END
        }
    }
}
