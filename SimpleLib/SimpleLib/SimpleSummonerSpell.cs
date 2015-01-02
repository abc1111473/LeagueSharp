using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SimpleLib
{
    public static class SimpleSummonerSpell
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spell Barrier = new Spell(Player.GetSpellSlot("SummonerBarrier"));
        public static Spell Clairvoyance = new Spell(Player.GetSpellSlot("SummonerClairvoyance"));
        public static Spell Clarity = new Spell(Player.GetSpellSlot("SummonerMana"), 600);
        public static Spell Cleanse = new Spell(Player.GetSpellSlot("SummonerBoost"));
        public static Spell Exhaust = new Spell(Player.GetSpellSlot("SummonerExhaust"), 650);
        public static Spell Flash = new Spell(Player.GetSpellSlot("SummonerFlash"), 400);
        public static Spell Ghost = new Spell(Player.GetSpellSlot("SummonerHaste"));
        public static Spell Heal = new Spell(Player.GetSpellSlot("SummonerHeal"), 700);
        public static Spell Ignite = new Spell(Player.GetSpellSlot("SummonerDot"), 600);
        public static Spell Revive = new Spell(Player.GetSpellSlot("SummonerRevive"));
        public static Spell Smite = new Spell(Player.GetSpellSlot("SummonerSmite"), 700);
        public static Spell Teleport = new Spell(Player.GetSpellSlot("SummonerTeleport"));

        public static bool HasSummoner(this Obj_AI_Hero target, Spell summonerSpell)
        {
            return summonerSpell.Slot != SpellSlot.Unknown;
        }

        public static bool CanUseSummoner(this Obj_AI_Hero target, Spell summonerSpell)
        {
            return HasSummoner(target, summonerSpell) && summonerSpell.IsReady();
        }

        public static void CastBarrier()
        {
            if (!Player.CanUseSummoner(Barrier))
            {
                return;
            }

            Barrier.Cast();
        }

        public static void CastBarrier(float healthPrecent)
        {
            if (!Player.CanUseSummoner(Barrier))
            {
                return;
            }

            if (Player.HealthPercentage() <= healthPrecent)
            {
                Barrier.Cast();
            }
        }

        public static void CastClairvoyance(Vector3 position)
        {
            if (!Player.CanUseSummoner(Clairvoyance))
            {
                return;
            }
            Clairvoyance.Cast(position);
        }

        public static void CastClarity()
        {
            if (!Player.CanUseSummoner(Clarity))
            {
                return;
            }

            Clarity.Cast();
        }

        public static void CastClarity(float manaProcent)
        {
            if (!Player.CanUseSummoner(Clarity))
            {
                return;
            }

            if (Player.ManaPercentage() <= manaProcent)
            {
                Clarity.Cast();
            }
        }

        public static void CastClarity(int numberOfAllys)
        {
            if (!Player.CanUseSummoner(Clarity))
            {
                return;
            }

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(600));

            if (temp.Count() >= numberOfAllys)
            {
                Clarity.Cast();
            }
        }

        public static void CastCleanse()
        {
            if (!Player.CanUseSummoner(Cleanse))
            {
                return;
            }

            Cleanse.Cast();
        }

        public static void CastExhaust(Obj_AI_Hero target)
        {
            if (!Player.CanUseSummoner(Exhaust))
            {
                return;
            }

            if (target.IsValidTarget(650))
            {
                Exhaust.Cast(target);
            }
        }

        public static void CastFlash(Vector3 position)
        {
            if (!Player.CanUseSummoner(Flash))
            {
                return;
            }

            Flash.Cast(position);
        }

        public static void CastGhost()
        {
            if (!Player.CanUseSummoner(Ghost))
            {
                return;
            }

            Ghost.Cast();
        }

        public static void CastHeal()
        {
            if (!Player.CanUseSummoner(Heal))
            {
                return;
            }

            Heal.Cast();
        }

        public static void CastHeal(float healtProcent)
        {
            if (!Player.CanUseSummoner(Heal))
            {
                return;
            }

            if (Player.HealthPercentage() <= healtProcent)
            {
                Heal.Cast();
            }
        }

        public static void CastHeal(int numberOfAllys)
        {
            if (!Player.CanUseSummoner(Heal))
            {
                return;
            }

            var temp = ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && ally.IsValidTarget(700));

            if (temp.Count() >= numberOfAllys)
            {
                Heal.Cast();
            }
        }

        public static void CastIgnite(Obj_AI_Hero target)
        {
            if (!Player.CanUseSummoner(Ignite))
            {
                return;
            }

            if (target.IsValidTarget(600))
            {
                Ignite.Cast(target);
            }
        }

        public static void CastRevive()
        {
            if (!Player.CanUseSummoner(Revive))
            {
                return;
            }

            Revive.Cast();
        }

        public static void CastSmite(Obj_AI_Base target)
        {
            if (!Player.CanUseSummoner(Smite))
            {
                return;
            }

            if (target.IsValidTarget(700))
            {
                Smite.Cast(target);
            }
        }

        public static void CastTeleport(Obj_AI_Base target)
        {
            if (!Player.CanUseSummoner(Teleport))
            {
                return;
            }

            Teleport.Cast(target);
        }
    }
}
