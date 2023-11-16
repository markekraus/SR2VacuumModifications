// Decompiled with JetBrains decompiler
// Type: Vacuum_Modifications.MyMod
// Assembly: Vac Modifications, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: B7ED2D59-9D1F-4D3C-81AA-D3E80A97CD8B
// Assembly location: D:\Steam\steamapps\common\Slime Rancher 2\Mods\Vac Modifications.dll

using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vacuum_Modifications
{
  public class MyMod : MelonMod
  {
    public static PlayerState _player;
    public static MelonPreferences_Category VacModifications;
    public static MelonPreferences_Entry<double> vacShootCooldown;
    public static MelonPreferences_Entry<int> playerCustomLimit;
    public static MelonPreferences_Entry<int> plortCollectorCustomLimit;
    public static MelonPreferences_Entry<int> feederCustomLimit;
    public static MelonPreferences_Entry<int> siloCustomLimit;
    public static MelonPreferences_Entry<bool> halfForMoreVaccablesModLargos;
    internal static IdentifiableTypeGroup largoGroup;
    public static bool isMoreVaccablesInstalled = MelonBase.FindMelon("MoreVaccablesMod", "KomiksPL") != null;

    public override void OnInitializeMelon()
    {
      MelonLogger.Msg("MoreVaccablesMod is " + (MyMod.isMoreVaccablesInstalled ? "Installed" : "Not Installed") + "!");
      MyMod.VacModifications = MelonPreferences.CreateCategory("Vac Modifications");
      MyMod.vacShootCooldown = MyMod.VacModifications.CreateEntry<double>("Vacuum Shoot Cooldown", 0.24, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
      MyMod.playerCustomLimit = MyMod.VacModifications.CreateEntry<int>("Player Vacuum Custom Item Limit", 100, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
      MyMod.plortCollectorCustomLimit = MyMod.VacModifications.CreateEntry<int>("Plort Collector Custom Item Limit", 100, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
      MyMod.feederCustomLimit = MyMod.VacModifications.CreateEntry<int>("Feeder Custom Item Limit", 100, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
      MyMod.siloCustomLimit = MyMod.VacModifications.CreateEntry<int>("Silo Custom Item Limit", 100, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
      if (!MyMod.isMoreVaccablesInstalled)
        return;
      MyMod.halfForMoreVaccablesModLargos = MyMod.VacModifications.CreateEntry<bool>("Half space for More Vaccables Mod Largos", true, (string) null, (string) null, false, false, (ValueValidator) null, (string) null);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
      if (!(sceneName == "PlayerCore"))
        return;
      MyMod._player = SRSingleton<SceneContext>.Instance.PlayerState;
      MyMod.largoGroup = MyMod.Get<IdentifiableTypeGroup>("LargoGroup");
    }

    public static int getAmmoModelType(AmmoModel ammoModel, int index, bool isLargo)
    {
      string name = ((Il2CppArrayBase<Ammo.Slot>) ammoModel.slots)[index].Definition.name;
      if (name.Contains("AmmoSlot"))
        return !isLargo || !MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.playerCustomLimit.Value : MyMod.playerCustomLimit.Value / 2;
      if (name.Contains("PlortCollector"))
        return !isLargo || !MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.plortCollectorCustomLimit.Value : MyMod.plortCollectorCustomLimit.Value / 2;
      if (name.Contains("Feeder"))
        return !isLargo || !MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.feederCustomLimit.Value : MyMod.feederCustomLimit.Value / 2;
      return name.Contains("Silo") ? (!isLargo || !MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.siloCustomLimit.Value : MyMod.siloCustomLimit.Value / 2) : (!isLargo || !MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.playerCustomLimit.Value : MyMod.playerCustomLimit.Value / 2);
    }

    public static int calculateMaxAmmo(AmmoModel ammoModel, int index, IdentifiableType id) => (id.ReferenceId.Equals("SlimeDefinition.Tarr") || MyMod.largoGroup.IsMember(id)) && MyMod.isMoreVaccablesInstalled && MyMod.halfForMoreVaccablesModLargos.Value ? MyMod.getAmmoModelType(ammoModel, index, true) : MyMod.getAmmoModelType(ammoModel, index, false);

    public static T Get<T>(string name) where T : UnityEngine.Object => ((IEnumerable<T>) Resources.FindObjectsOfTypeAll<T>()).FirstOrDefault<T>((Func<T, bool>) (x => x.name == name));

    //[HarmonyPatch(typeof (WeaponVacuum), nameof(WeaponVacuum.Expel), new System.Type[] {})]
    [HarmonyPatch(typeof (WeaponVacuum), nameof(WeaponVacuum.Expel), new System.Type[] {typeof(GameObject), typeof(bool), typeof(float), typeof(SlimeAppearance.AppearanceSaveSet)})]
    private static class WeapeonVacuum_Expel_Patch
    {
      private static void Postfix() => MyMod._player.Vacuum.ShootCooldown = (float) MyMod.vacShootCooldown.Value;
    }

    [HarmonyPatch(typeof (AmmoModel), nameof(AmmoModel.GetSlotMaxCount))]
    private static class AmmoModel_GetSlotMaxCount_Patch
    {
      public static void Postfix(
        AmmoModel __instance,
        IdentifiableType id,
        int index,
        ref int __result)
      {
        if (!((UnityEngine.Object) id != (UnityEngine.Object) null))
          return;
        int maxAmmo = MyMod.calculateMaxAmmo(__instance, index, id);
        __result = maxAmmo;
      }
    }
  }
}
