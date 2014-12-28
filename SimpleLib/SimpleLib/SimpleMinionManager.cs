using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SimpleLib
{
    public class SMM
    {
        public enum MinionMode
        {
            LastHit,
            LaneClear,
            LaneFreez,
            Closest,
            Furthest,
            NearMouse
        }

        public enum MinionTeam
        {
            Neutral,
            Ally,
            Enemy,
            All
        }

        public enum MinionType
        {
            Ranged,
            Melee,
            Siege,
            Super,
            Ward,
            All,
            None
        }

        /// <summary>
        ///     Returns the List(Obj_AI_Base) of all enemy minions in Range
        /// </summary>
        public static List<Obj_AI_Base> EnemyMinions(float range)
        {
            var result = new List<Obj_AI_Base>();
            foreach (
                var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(minion => minion.IsValidTarget(range) && minion.IsEnemy && minion.Name != "Beacon"))
            {
                result.Add(minion);
            }
            return result;
        }

        /// <summary>
        ///     Returns the List(Obj_AI_Base) of all ally minions in Range
        /// </summary>
        public static List<Obj_AI_Base> AllyMinions(float range)
        {
            var result = new List<Obj_AI_Base>();
            foreach (
                var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(minion => minion.IsValidTarget(range) && minion.IsAlly && minion.Name != "Beacon"))
            {
                result.Add(minion);
            }
            return result;
        }

        /// <summary>
        ///     Returns the List(Obj_AI_Base) of all neutral monsters in Range and ExtendMonitarRange
        /// </summary>
        public static List<Obj_AI_Base> NeutralMinions(float range)
        {
            var result = new List<Obj_AI_Base>();
            foreach (
                var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.IsValidTarget(range) && minion.Team == GameObjectTeam.Neutral &&
                                minion.Name != "Beacon"))
            {
                result.Add(minion);
            }
            return result;
        }

        /// <summary>
        ///     Returns the List(Obj_AI_Base) of all minions in Range
        /// </summary>
        public static List<Obj_AI_Base> AllMinions(float range)
        {
            var result = new List<Obj_AI_Base>();
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(range)))
            {
                result.Add(minion);
            }
            return result;
        }

        /// <summary>
        ///     Cheaks if unit is a minion or if includeWards = true cheaks for wards as well.
        /// </summary>
        public static bool IsMinion(Obj_AI_Base unit, bool includeWards = false)
        {
            var name = unit.BaseSkinName.ToLower();

            if (name.Contains("minion"))
            {
                return true;
            }

            if (!includeWards)
            {
                return false;
            }

            return name.Contains("ward") || name.Contains("trinket");
        }

        /// <summary>
        ///     Cheaks if unit is a jungle monster.
        /// </summary>
        public static bool IsMonster(Obj_AI_Base unit)
        {
            return unit.Team == GameObjectTeam.Neutral;
        }

        /// <summary>
        ///     Returns the Type of minion for unit
        /// </summary>
        public static MinionType GetType(Obj_AI_Base unit)
        {
            if (!IsMinion(unit, true))
            {
                return MinionType.None;
            }

            var name = unit.BaseSkinName.ToLower();

            if (name.Contains("super"))
            {
                return MinionType.Super;
            }
            if (name.Contains("siege") || name.Contains("cannon"))
            {
                return MinionType.Siege;
            }
            if (name.Contains("melee"))
            {
                return MinionType.Melee;
            }
            if (name.Contains("ranged") || name.Contains("wizard") || name.Contains("caster"))
            {
                return MinionType.Ranged;
            }
            if (name.Contains("ward") || name.Contains("trinket"))
            {
                return MinionType.Ward;
            }
            return MinionType.None;
        }

        /// <summary>
        ///     Returns the priorety for the unit
        /// </summary>
        public static int MinionPriorety(Obj_AI_Base unit)
        {
            var type = GetType(unit);
            switch (type)
            {
                case MinionType.Ward:
                    return 1;
                case MinionType.Ranged:
                    return 2;
                case MinionType.Melee:
                    return 3;
                case MinionType.Siege:
                    return 4;
                case MinionType.Super:
                    return 5;
                default:
                    return 1;
            }
        }

        /// <summary>
        ///     Returns the priorety for the set type
        /// </summary>
        public static int MinionPriorety(MinionType type)
        {
            switch (type)
            {
                case MinionType.Ward:
                    return 1;
                case MinionType.Ranged:
                    return 2;
                case MinionType.Melee:
                    return 3;
                case MinionType.Siege:
                    return 4;
                case MinionType.Super:
                    return 5;
                default:
                    return 1;
            }
        }

        /// <summary>
        ///     Returns the priorety for the set unit inside that champ.
        /// </summary>
        public static int JunglePriorety(Obj_AI_Base unit)
        {
            var name = unit.BaseSkinName.ToLower();

            if (name == "sru_krugmini" || name == "sru_murkwolfmini" || name == "sru_crab" ||
                name == "sru_razorbeakmini" || name == "sru_redmini" || name == "sru_bluemini")
            {
                return 1;
            }

            if (name == "sru_krug" || name == "sru_murkwolf" || name == "sru_gromp" || name == "sru_razorbeak")
            {
                return 2;
            }

            if (name == "sru_red" || name == "sru_blue")
            {
                return 3;
            }

            if (name == "sru_dragon")
            {
                return 4;
            }

            if (name == "sru_baron")
            {
                return 5;
            }
            return 0;
        }

        /// <summary>
        ///     Returns the minion for the set params
        /// </summary>
        public static Obj_AI_Base GetMinion(float selectRange,
            MinionMode selectMode,
            bool includeWards = false,
            int farmDelay = 70,
            MinionTeam selectTeam = MinionTeam.Enemy)
        {
            Obj_AI_Base tempMinion = null;

            if (selectMode == MinionMode.LastHit)
            {
                if (selectTeam == MinionTeam.Ally)
                {
                    return null;
                }

                List<Obj_AI_Base> tempList = LastHitMinions(selectRange, selectTeam, farmDelay);

                if (!tempList.Any())
                    return null;

                foreach (var minion in tempList)
                {
                    if (SimpleCollision.YasuoWallCollision(selectRange, minion))
                    {
                        continue;
                    }

                    if (tempMinion == null)
                    {
                        tempMinion = minion;
                        continue;
                    }

                    if (minion.Team != GameObjectTeam.Neutral && MinionPriorety(tempMinion) < MinionPriorety(minion))
                    {
                        tempMinion = minion;
                        continue;
                    }

                    if (minion.Team == GameObjectTeam.Neutral && JunglePriorety(tempMinion) < JunglePriorety(minion))
                    {
                        tempMinion = minion;
                    }
                }
                return tempMinion;
            }

            if (selectMode == MinionMode.LaneClear)
            {
                if (selectTeam == MinionTeam.Ally)
                {
                    return null;
                }

                List<Obj_AI_Base> tempList = new List<Obj_AI_Base>();

                foreach (var minion in EnemyMinions(selectRange))
                {
                    if (!SimpleCollision.YasuoWallCollision(selectRange, minion) && IsMinion(minion))
                    {
                        tempList.Add(minion);
                    }
                }

                foreach (var minion in NeutralMinions(selectRange))
                {
                    if (!SimpleCollision.YasuoWallCollision(selectRange, minion) && IsMinion(minion))
                    {
                        tempList.Add(minion);
                    }
                }

                foreach (var minion in tempList)
                {
                    if (tempMinion == null)
                    {
                        tempMinion = minion;
                    }

                    if ((minion.Health - ObjectManager.Player.GetAutoAttackDamage(minion) * 1.5f) >
                        (tempMinion.Health - ObjectManager.Player.GetAutoAttackDamage(minion) * 1.5f))
                    {
                        tempMinion = minion;
                    }
                }
                return tempMinion;
            }

            if (selectMode == MinionMode.LaneFreez)
            {
                if (selectTeam == MinionTeam.Ally)
                {
                    return null;
                }

                List<Obj_AI_Base> tempAllyMinions = new List<Obj_AI_Base>();
                List<Obj_AI_Base> tempEnemyMinions = new List<Obj_AI_Base>();

                foreach (var minion in EnemyMinions(selectRange))
                {
                    tempEnemyMinions.Add(minion);
                }

                foreach (var minion in AllyMinions(selectRange))
                {
                    tempAllyMinions.Add(minion);
                }

                if (tempAllyMinions.Count + 5 < tempEnemyMinions.Count)
                {
                    tempMinion = GetMinion(selectRange, MinionMode.LaneClear, includeWards, farmDelay, selectTeam);
                }
                else
                {
                    tempMinion = GetMinion(selectRange, MinionMode.LastHit, includeWards, farmDelay, selectTeam);
                }

                return tempMinion;
            }

            if (selectMode == MinionMode.NearMouse || selectMode == MinionMode.Closest ||
                selectMode == MinionMode.Furthest)
            {
                switch (selectTeam)
                {
                    case MinionTeam.Enemy:

                        foreach (var minion in EnemyMinions(selectRange))
                        {
                            if (!IsMinion(minion, includeWards))
                            {
                                continue;
                            }

                            if (tempMinion == null)
                            {
                                tempMinion = minion;
                                continue;
                            }

                            if ((selectMode == MinionMode.NearMouse &&
                                 tempMinion.Distance(Game.CursorPos) > minion.Distance(Game.CursorPos)) ||
                                (selectMode == MinionMode.Closest &&
                                 tempMinion.Distance(ObjectManager.Player) > minion.Distance(ObjectManager.Player)) ||
                                (selectMode == MinionMode.Furthest &&
                                 tempMinion.Distance(ObjectManager.Player) < minion.Distance(ObjectManager.Player)))
                            {
                                tempMinion = minion;
                            }
                        }
                        return tempMinion;

                    case MinionTeam.Neutral:

                        foreach (var minion in NeutralMinions(selectRange))
                        {
                            if (!IsMinion(minion, includeWards))
                            {
                                continue;
                            }

                            if (tempMinion == null)
                            {
                                tempMinion = minion;
                                continue;
                            }

                            if ((selectMode == MinionMode.NearMouse &&
                                 tempMinion.Distance(Game.CursorPos) > minion.Distance(Game.CursorPos)) ||
                                (selectMode == MinionMode.Closest &&
                                 tempMinion.Distance(ObjectManager.Player) > minion.Distance(ObjectManager.Player)) ||
                                (selectMode == MinionMode.Furthest &&
                                 tempMinion.Distance(ObjectManager.Player) < minion.Distance(ObjectManager.Player)))
                            {
                                tempMinion = minion;
                            }
                        }
                        return tempMinion;

                    case MinionTeam.Ally:

                        foreach (var minion in AllyMinions(selectRange))
                        {
                            if (!IsMinion(minion, includeWards))
                            {
                                continue;
                            }

                            if (tempMinion == null)
                            {
                                tempMinion = minion;
                                continue;
                            }

                            if ((selectMode == MinionMode.NearMouse &&
                                 tempMinion.Distance(Game.CursorPos) > minion.Distance(Game.CursorPos)) ||
                                (selectMode == MinionMode.Closest &&
                                 tempMinion.Distance(ObjectManager.Player) > minion.Distance(ObjectManager.Player)) ||
                                (selectMode == MinionMode.Furthest &&
                                 tempMinion.Distance(ObjectManager.Player) < minion.Distance(ObjectManager.Player)))
                            {
                                tempMinion = minion;
                            }
                        }
                        return tempMinion;

                    default:

                        foreach (var minion in AllMinions(selectRange))
                        {
                            if (!IsMinion(minion, includeWards))
                            {
                                continue;
                            }

                            if (tempMinion == null)
                            {
                                tempMinion = minion;
                                continue;
                            }

                            if ((selectMode == MinionMode.NearMouse &&
                                 tempMinion.Distance(Game.CursorPos) > minion.Distance(Game.CursorPos)) ||
                                (selectMode == MinionMode.Closest &&
                                 tempMinion.Distance(ObjectManager.Player) > minion.Distance(ObjectManager.Player)) ||
                                (selectMode == MinionMode.Furthest &&
                                 tempMinion.Distance(ObjectManager.Player) < minion.Distance(ObjectManager.Player)))
                            {
                                tempMinion = minion;
                            }
                        }
                        return tempMinion;
                }
            }
            return null;
        }

        /// <summary>
        ///     Returns whether a minion will soon be for lasthit with AA.
        /// </summary>
        public static bool ShouldWait(float range, int farmDelay = 70)
        {
            return
                EnemyMinions(range)
                    .Any(
                        minion =>
                            HealthPrediction.LaneClearHealthPrediction(
                                minion,
                                ((int) (ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                                 1000 * (int) ObjectManager.Player.Distance(minion) / (int) ObjectManager.Player.BasicAttack.MissileSpeed),
                                farmDelay) <= ObjectManager.Player.GetAutoAttackDamage(minion));
        }

        private static List<Obj_AI_Base> LastHitMinions(float range, MinionTeam selectTeam, int farmDelay = 70)
        {
            List<Obj_AI_Base> tempList = new List<Obj_AI_Base>();

            if (selectTeam == MinionTeam.Enemy || selectTeam == MinionTeam.Neutral)
            {
                foreach (var minion in EnemyMinions(range))
                {
                    if (!IsMinion(minion))
                    {
                        continue;
                    }

                    var predHealth = HealthPrediction.GetHealthPrediction(
                        minion,
                        ((int) (ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                         1000 * (int) ObjectManager.Player.Distance(minion) / (int) ObjectManager.Player.BasicAttack.MissileSpeed), farmDelay);
                    if (predHealth > 0 && (predHealth + 2f) <= ObjectManager.Player.GetAutoAttackDamage(minion, true))
                    {
                        tempList.Add(minion);
                    }
                }

                foreach (var minion in NeutralMinions(range))
                {
                    if (!IsMonster(minion))
                    {
                        continue;
                    }

                    var predHealth = HealthPrediction.GetHealthPrediction(
                        minion,
                        ((int) (ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                         1000 * (int) ObjectManager.Player.Distance(minion) / (int) ObjectManager.Player.BasicAttack.MissileSpeed), farmDelay);
                    if (predHealth > 0 && (predHealth + 2f) <= ObjectManager.Player.GetAutoAttackDamage(minion, true))
                    {
                        tempList.Add(minion);
                    }
                }

                return tempList;
            }

            if (selectTeam == MinionTeam.Ally)
            {
                foreach (var minion in AllyMinions(range))
                {
                    if (!IsMonster(minion))
                    {
                        continue;
                    }

                    var predHealth = HealthPrediction.GetHealthPrediction(
                        minion,
                        ((int) (ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                         1000 * (int) ObjectManager.Player.Distance(minion) / (int) ObjectManager.Player.BasicAttack.MissileSpeed), farmDelay);
                    if (predHealth > 0 && (predHealth + 2f) <= ObjectManager.Player.GetAutoAttackDamage(minion, true))
                    {
                        tempList.Add(minion);
                    }
                }
                return tempList;
            }

            foreach (var minion in AllMinions(range))
            {
                if (!IsMonster(minion))
                {
                    continue;
                }

                var predHealth = HealthPrediction.GetHealthPrediction(
                    minion,
                    ((int) (ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                     1000 * (int) ObjectManager.Player.Distance(minion) / (int) ObjectManager.Player.BasicAttack.MissileSpeed), farmDelay);
                if (predHealth > 0 && (predHealth + 2f) <= ObjectManager.Player.GetAutoAttackDamage(minion, true))
                {
                    tempList.Add(minion);
                }
            }
            return tempList;
        }
    }
}