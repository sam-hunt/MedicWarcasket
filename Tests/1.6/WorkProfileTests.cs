using System.Collections.Generic;
using System.Linq;
using Verse;
using Xunit;

namespace MedicWarcasket.Tests;

// The medic trait's work profile (WorkProfile.Derive and Compose) against bare work type defs
// shaped as vanilla's: which work types the settings may toggle, their defaults, and the tags
// and types a choice writes. VFEP's unpatched tags must give the shipped trait (Childcare
// listed, Intellectual still a tag); opening Research must clear the Intellectual tag and list
// Dark study instead. Patch-mod profiles follow Docs/Specs/Medic-Work-Trait.md 4.3.
public class WorkProfileTests
{
    private const WorkTags VfepTags = WorkTags.ManualSkilled | WorkTags.ManualDumb | WorkTags.Animals
        | WorkTags.Artistic | WorkTags.Caring | WorkTags.Intellectual;

    private const WorkTags ShippedTags = WorkTags.ManualSkilled | WorkTags.Cleaning | WorkTags.Animals
        | WorkTags.Artistic | WorkTags.Intellectual;

    private static WorkTypeDef Work(string defName, WorkTags tags) =>
        new() { defName = defName, workTags = tags | WorkTags.AllWork };

    private static readonly WorkTypeDef Doctor = Work("Doctor", WorkTags.Caring | WorkTags.Commoner);
    private static readonly WorkTypeDef Childcare = Work("Childcare", WorkTags.Caring);
    private static readonly WorkTypeDef Research = Work("Research", WorkTags.Intellectual | WorkTags.Commoner);
    private static readonly WorkTypeDef DarkStudy = Work("DarkStudy", WorkTags.Intellectual | WorkTags.Commoner);
    private static readonly WorkTypeDef Hauling = Work("Hauling", WorkTags.ManualDumb | WorkTags.Hauling);
    private static readonly WorkTypeDef Crafting = Work("Crafting", WorkTags.ManualSkilled | WorkTags.Crafting);
    private static readonly WorkTypeDef Both = Work("ModdedBoth", WorkTags.Caring | WorkTags.Intellectual);
    private static readonly WorkTypeDef CaringCrafter = Work("ModdedCaringCrafter", WorkTags.Caring | WorkTags.ManualSkilled);

    private static readonly List<WorkTypeDef> All = [Doctor, Childcare, Hauling, Crafting, Research, DarkStudy];

    private static WorkProfile.Profile Vfep() => WorkProfile.Derive(VfepTags, [], All, Doctor);

    private static bool Only(WorkTypeDef work, params WorkTypeDef[] open) => open.Contains(work);

    [Fact]
    public void Derive_TogglesCaringAndIntellectualInWorkTabOrder()
    {
        WorkProfile.Profile profile = Vfep();
        Assert.Equal([Doctor, Childcare, Research, DarkStudy], profile.toggleable);
        Assert.Equal([Doctor], profile.openByDefault);
        Assert.Equal(ShippedTags, profile.baseTags);
        Assert.Empty(profile.baseTypes);
    }

    [Fact]
    public void Compose_DefaultsGiveTheShippedTrait()
    {
        WorkProfile.Profile profile = Vfep();
        (WorkTags tags, List<WorkTypeDef> types) = WorkProfile.Compose(profile, profile.openByDefault.Contains);
        Assert.Equal(ShippedTags, tags);
        Assert.Equal([Childcare], types);
    }

    [Fact]
    public void Compose_OpeningResearchClearsTheTagAndListsDarkStudy()
    {
        (WorkTags tags, List<WorkTypeDef> types) = WorkProfile.Compose(Vfep(), work => Only(work, Doctor, Research));
        Assert.Equal(ShippedTags & ~WorkTags.Intellectual, tags);
        Assert.Equal([Childcare, DarkStudy], types);
    }

    [Fact]
    public void Compose_ClosingEveryIntellectualTypeKeepsTheTag()
    {
        (WorkTags tags, List<WorkTypeDef> types) = WorkProfile.Compose(Vfep(), work => Only(work, Childcare));
        Assert.Equal(ShippedTags, tags);
        Assert.Equal([Doctor], types);
    }

    [Fact]
    public void Derive_WithoutVfepTags_OpensEverythingByDefault()
    {
        WorkProfile.Profile profile = WorkProfile.Derive(WorkTags.None, [], All, Doctor);
        Assert.Equal([Doctor, Childcare, Research, DarkStudy], profile.openByDefault.OrderBy(All.IndexOf));
        (WorkTags tags, List<WorkTypeDef> types) = WorkProfile.Compose(profile, profile.openByDefault.Contains);
        Assert.Equal(WorkTags.None, tags);
        Assert.Empty(types);

        (tags, types) = WorkProfile.Compose(profile, work => Only(work, Doctor));
        Assert.Equal(WorkTags.None, tags);
        Assert.Equal([Childcare, Research, DarkStudy], types);
    }

    [Fact]
    public void Derive_VfepListedType_IsToggleableAndClosedByDefault()
    {
        WorkProfile.Profile profile = WorkProfile.Derive(WorkTags.None, [Research, Crafting], All, Doctor);
        Assert.Contains(Research, profile.toggleable);
        Assert.DoesNotContain(Research, profile.openByDefault);
        Assert.Equal([Crafting], profile.baseTypes);
    }

    [Fact]
    public void Derive_TypeCarryingBothTags_IsListedOnceAndOpeningItClearsTheTag()
    {
        List<WorkTypeDef> all = [Doctor, Both, Research];
        WorkProfile.Profile profile = WorkProfile.Derive(VfepTags, [], all, Doctor);
        Assert.Equal([Doctor, Both, Research], profile.toggleable);
        Assert.DoesNotContain(Both, profile.openByDefault);

        (WorkTags tags, List<WorkTypeDef> types) = WorkProfile.Compose(profile, work => Only(work, Doctor, Both));
        Assert.Equal(WorkTags.None, tags & WorkTags.Intellectual);
        Assert.Equal([Research], types);
    }

    [Fact]
    public void Derive_TypeBlockedByAnotherTag_IsNotToggleable()
    {
        WorkProfile.Profile profile = WorkProfile.Derive(VfepTags, [], [Doctor, CaringCrafter], Doctor);
        Assert.Equal([Doctor], profile.toggleable);
    }
}
