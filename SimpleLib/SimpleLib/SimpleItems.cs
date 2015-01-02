using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleLib
{
    public  static class SimpleItems
    {
        //Active items
        public static readonly Items.Item BannerofCommand = ItemData.Banner_of_Command.GetItem();
        public static readonly Items.Item BilgewaterCutlass = ItemData.Bilgewater_Cutlass.GetItem();
        public static readonly Items.Item BlackfireTorch = ItemData.Blackfire_Torch.GetItem();
        public static readonly Items.Item BladeOfTheRuinedKing = ItemData.Blade_of_the_Ruined_King.GetItem();
        public static readonly Items.Item DeathFireGrasp = ItemData.Deathfire_Grasp.GetItem();
        public static readonly Items.Item Entropy = ItemData.Entropy.GetItem();
        public static readonly Items.Item FaceOfTheMountain = ItemData.Face_of_the_Mountain.GetItem();
        public static readonly Items.Item FrostQueensClaim = ItemData.Frost_Queens_Claim.GetItem();
        public static readonly Items.Item GrezsSpectralLantern = ItemData.Grezs_Spectral_Lantern.GetItem();
        public static readonly Items.Item HextechGunblade = ItemData.Hextech_Gunblade.GetItem();
        public static readonly Items.Item HextechSweeper = ItemData.Hextech_Sweeper.GetItem();
        public static readonly Items.Item TheLightbringer = ItemData.The_Lightbringer.GetItem();
        public static readonly Items.Item LocketOfTheIronSolari = ItemData.Locket_of_the_Iron_Solari.GetItem();
        public static readonly Items.Item MercurialScimitar = ItemData.Mercurial_Scimitar.GetItem();
        public static readonly Items.Item MikaelsCrucible = ItemData.Mikaels_Crucible.GetItem();
        public static readonly Items.Item OdynsVeil = ItemData.Odyns_Veil.GetItem();
        public static readonly Items.Item Ohmwrecker = ItemData.Ohmwrecker.GetItem();
        public static readonly Items.Item QuicksilverSash = ItemData.Quicksilver_Sash.GetItem();
        public static readonly Items.Item RanduinsOmen = ItemData.Randuins_Omen.GetItem();
        public static readonly Items.Item RavenousHydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
        public static readonly Items.Item RighteousGlory = ItemData.Righteous_Glory.GetItem();
        public static readonly Items.Item SeraphsEmbrace = ItemData.Seraphs_Embrace.GetItem();
        public static readonly Items.Item SeraphsEmbrace2 = ItemData.Seraphs_Embrace2.GetItem();
        public static readonly Items.Item SwordOfTheDivine = ItemData.Sword_of_the_Divine.GetItem();
        public static readonly Items.Item TalismanOfAscension = ItemData.Talisman_of_Ascension.GetItem();
        public static readonly Items.Item TwinShadows = ItemData.Twin_Shadows.GetItem();
        public static readonly Items.Item TwinShadows2 = ItemData.Twin_Shadows2.GetItem();
        public static readonly Items.Item YoumuusGhostblade = ItemData.Youmuus_Ghostblade.GetItem();
        public static readonly Items.Item ZhonyasHourglass = ItemData.Zhonyas_Hourglass.GetItem();
        
        //Consumables
        public static readonly Items.Item HealthPotion = ItemData.Health_Potion.GetItem();
        public static readonly Items.Item ManaPotion = ItemData.Mana_Potion.GetItem();
        public static readonly Items.Item StealthWard = ItemData.Stealth_Ward.GetItem();
        public static readonly Items.Item VisionWard = ItemData.Vision_Ward.GetItem();
        public static readonly Items.Item OraclesExtract = ItemData.Oracles_Extract.GetItem();
        public static readonly Items.Item OraclesElixir = ItemData.Oracles_Elixir.GetItem();
        public static readonly Items.Item CrystallineFlask = ItemData.Crystalline_Flask.GetItem();
        public static readonly Items.Item Sightstone = ItemData.Sightstone.GetItem();
        public static readonly Items.Item RubySightstone = ItemData.Ruby_Sightstone.GetItem();
        public static readonly Items.Item ElixirOfAgility = ItemData.Elixir_of_Agility.GetItem();
        public static readonly Items.Item ElixirOfBrilliance = ItemData.Elixir_of_Brilliance.GetItem();
        public static readonly Items.Item ElixirOfFortitude = ItemData.Elixir_of_Fortitude.GetItem();
        public static readonly Items.Item ElixirOfIron = ItemData.Elixir_of_Iron.GetItem();
        public static readonly Items.Item ElixirOfRuin = ItemData.Elixir_of_Ruin.GetItem();
        public static readonly Items.Item ElixirOfSorcery = ItemData.Elixir_of_Sorcery.GetItem();
        public static readonly Items.Item ElixirOfWrath = ItemData.Elixir_of_Wrath.GetItem();
        public static readonly Items.Item PoroSnax = ItemData.PoroSnax.GetItem();
        public static readonly Items.Item TotalBiscuitOfRejuvenation = ItemData.Total_Biscuit_of_Rejuvenation.GetItem();
        public static readonly Items.Item TotalBiscuitOfRejuvenation2 = ItemData.Total_Biscuit_of_Rejuvenation2.GetItem();

        //Toggle
        public static readonly Items.Item Muramana = ItemData.Muramana.GetItem();
        public static readonly Items.Item Muramana2 = ItemData.Muramana2.GetItem();

        public static bool HasItem(this Obj_AI_Hero target, Items.Item item)
        {
            return Items.HasItem(item.Id, target);
        }

        public static bool CanUseItem(this Obj_AI_Hero target, Items.Item item)
        {
            return HasItem(target, item) && Items.CanUseItem(item.Id);
        }
    }
}
