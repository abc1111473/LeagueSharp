using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SimpleLib
{
    namespace SimpleMinionManager
    {
        public class SMM
        {
            public enum MinionOrderTypes
            {
                None,
                Health,
                MaxHealth,
            }

            public enum MinionTeam
            {
                Neutral,
                Ally,
                Enemy,
                All,
            }

            public enum MinionTypes
            {
                Ranged,
                Melee,
                Siege,
                Super,
                All,                 
            }

            public enum MinionMode
            {
                LastHit,
                LaneClear,
                LaneFreez,
                Closest,
                Furthest,
                NearMouse,
                None,
            }

            private static Obj_AI_Hero Self = ObjectManager.Player;
            private static int _farmDelay;
            public static Obj_AI_Base SelectedMinion = null;
            private static Obj_AI_Base _focustMinion = null;

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange)))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all enemy minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllEnemyMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsEnemy && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ally minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllAllyMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsAlly && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all neutral monsters in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllNeutralMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.Team == GameObjectTeam.Neutral && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result; ;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ranged enemy minions in Range and ExtendMonitarRange.
            ///     It dosent includ siege or cannon minions.
            /// </summary>
            public static List<Obj_AI_Base> EnemyRangedMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsEnemy && (minion.Name.Contains("ranged") || minion.Name.Contains("wizard") || minion.Name.Contains("caster") && minion.Name != "Beacon")))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all enemy melee minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> EnemyMeleeMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsEnemy && minion.Name.Contains("melee") && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all enemy siege or cannon minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> EnemySiegeMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsEnemy && (minion.Name.Contains("siege") || minion.Name.Contains("cannon")) && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all enemy super minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> EnemySuperMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsEnemy && minion.Name.Contains("super") && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ranged ally minions in Range and ExtendMonitarRange.
            ///     It dosent includ siege or cannon minions.
            /// </summary>
            public static List<Obj_AI_Base> AllyRangedMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsAlly && (minion.Name.Contains("ranged") || minion.Name.Contains("wizard") || minion.Name.Contains("caster")) && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ally melee minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllyMeleeMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsAlly && minion.Name.Contains("melee") && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ally siege or cannon minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllySiegeMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsAlly && (minion.Name.Contains("siege") || minion.Name.Contains("cannon")) && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Returns the List<Obj_AI_Base> of all ally super minions in Range and ExtendMonitarRange
            /// </summary>
            public static List<Obj_AI_Base> AllySuperMinions(float Range, float ExtendMonitarRange = 0)
            {
                var result = new List<Obj_AI_Base>();

                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(Range + ExtendMonitarRange) && minion.IsAlly && minion.Name.Contains("super") && minion.Name != "Beacon"))
                {
                    result.Add(minion);
                }
                return result;
            }

            /// <summary>
            ///     Initializes the SMM. It subscribes it to Obj_AI_Turret_OnProcessSpellCast 
            ///     so it can use advanced under tower logic.
            /// </summary>
            public static void InitializeSMM()
            {
                Obj_AI_Turret.OnProcessSpellCast += Obj_AI_Turret_OnProcessSpellCast;
            }  

            private static int GetPrioretyForMinions(string targetName)
            {
                if (targetName.Contains("super")) return 4;
                else if (targetName.Contains("siege") || targetName.Contains("cannon")) return 3;
                else if (targetName.Contains("melee")) return 2;
                else if (targetName.Contains("ranged") || targetName.Contains("wizard") || targetName.Contains("caster")) return 1;
                else return 0;
            }

            private static int GetPrioretyForNeutralMonsters(string targetName)
            {
                if (targetName.Contains("Baron")) return 4;
                else if (targetName.Contains("Dragon")) return 3;
                else if (targetName.Contains("Red") || targetName.Contains("Blue")) return 2;
                else if (targetName.Contains("wolf") || targetName.Contains("Gromp") || targetName.Contains("Krug")) return 1;
                else return 0;
            }

            private static Obj_AI_Base CompereLessHealth(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (target_1.Health < target_2.Health) return target_1;

                if (target_1.Health == target_2.Health) return null;

                return target_2;
            }

            private static Obj_AI_Base CompereMoreHealth(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (target_1.Health < target_2.Health) return target_2;

                if (target_1.Health == target_2.Health) return null;

                return target_1;
            }

            private static Obj_AI_Base ComperePriorety(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (GetPrioretyForMinions(target_1.Name) < GetPrioretyForMinions(target_2.Name)) return target_2;

                if (GetPrioretyForMinions(target_1.Name) == GetPrioretyForMinions(target_2.Name)) return null;

                return target_1;
            }

            private static Obj_AI_Base CompereNeutralMonsterPriorety(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (GetPrioretyForNeutralMonsters(target_1.Name) < GetPrioretyForNeutralMonsters(target_2.Name)) return target_2;

                if (GetPrioretyForNeutralMonsters(target_1.Name) == GetPrioretyForNeutralMonsters(target_2.Name)) return null;

                return target_1;
            }

            private static Obj_AI_Base CompereClosest(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (Geometry.Distance(target_1, Self) > Geometry.Distance(target_2, Self)) return target_2;

                if (Geometry.Distance(target_1, Self) == Geometry.Distance(target_2, Self)) return null;

                return target_1;
            }

            private static Obj_AI_Base CompereFurthest(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (Geometry.Distance(target_1, Self) > Geometry.Distance(target_2, Self)) return target_1;

                if (Geometry.Distance(target_1, Self) == Geometry.Distance(target_2, Self)) return null;

                return target_2;
            }

            private static Obj_AI_Base CompereNearMouse(Obj_AI_Base target_1, Obj_AI_Base target_2)
            {
                if (Geometry.Distance(target_1, Game.CursorPos) > Geometry.Distance(target_2, Game.CursorPos)) return target_2;

                if (Geometry.Distance(target_1, Game.CursorPos) == Geometry.Distance(target_2, Game.CursorPos)) return null;

                return target_1;
            }

            /// <summary>
            ///     Checks if target is in tower range bouth ally and enemy.
            /// </summary>
            public static bool CheckIfUnderTower(Obj_AI_Base targert)
            {
                foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(775f, false, targert.Position)))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     Checks if target is in ally tower range.
            /// </summary>
            public static bool CheckIfUnderAllyTower(Obj_AI_Base targert)
            {
                foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(775f, false, targert.Position) && turret.IsAlly))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     Checks if target is in enemy tower range.
            /// </summary>
            public static bool CheckIfUnderEnemyTower(Obj_AI_Base targert)
            {
                foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(775f, false, targert.Position) && turret.IsEnemy))
                {
                    return true;
                }
                return false;
            }

            private static Obj_AI_Base GetLastHitMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                List<Obj_AI_Base> newTarget = new List<Obj_AI_Base>();
                Obj_AI_Base tempTarget = null;

                switch(selectTime)
                {
                    case MinionTeam.Enemy:
                        foreach (var minion in AllEnemyMinions(Range + ExtendMonitarRange))
                        {
                            var predHealth = HealthPrediction.GetHealthPrediction(minion, ((int)(Self.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Self.Distance(minion) / (int)Self.BasicAttack.MissileSpeed), _farmDelay);
                            if(predHealth > 0 && predHealth <= Self.GetAutoAttackDamage(minion, true))
                            {
                                newTarget.Add(minion);
                            }                                   
                        }
                        if (newTarget.Count == 0) return null;
                        else if (newTarget.Count == 1) return newTarget.First<Obj_AI_Base>();
                        else
                        {
                            foreach (var minion in newTarget)
                            {
                                if (tempTarget == null) tempTarget = minion;
                                else
                                {
                                    if (ComperePriorety(tempTarget, minion) != null) tempTarget = ComperePriorety(tempTarget, minion);
                                    else if (CompereLessHealth(tempTarget, minion) != null) tempTarget = CompereLessHealth(tempTarget, minion);
                                    else if (CompereClosest(tempTarget, minion) != null) tempTarget = CompereClosest(tempTarget, minion);
                                    else if (CompereNearMouse(tempTarget, minion) != null) tempTarget = CompereNearMouse(tempTarget, minion);
                                    else tempTarget = minion;
                                }
                            }
                            return tempTarget;
                        }

                    case MinionTeam.Neutral:
                        foreach (var minion in AllNeutralMinions(Range + ExtendMonitarRange))
                        {
                            var predHealth = HealthPrediction.GetHealthPrediction(minion, ((int)(Self.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Self.Distance(minion) / (int)Self.BasicAttack.MissileSpeed), _farmDelay);
                            if(predHealth > 0 && predHealth <= Self.GetAutoAttackDamage(minion, true))
                            {
                                newTarget.Add(minion);
                            }                                   
                        }
                        if (newTarget.Count == 0) return null;
                        else if (newTarget.Count == 1) return newTarget.First<Obj_AI_Base>();
                        else
                        {
                            
                            foreach(var minion in newTarget)
                            {
                                if (tempTarget == null) tempTarget = minion;
                                else
                                {
                                    if (CompereNeutralMonsterPriorety(tempTarget, minion) != null) tempTarget = CompereNeutralMonsterPriorety(tempTarget, minion);
                                    else if (CompereLessHealth(tempTarget, minion) != null) tempTarget = CompereLessHealth(tempTarget, minion);
                                    else if (CompereClosest(tempTarget, minion) != null) tempTarget = CompereClosest(tempTarget, minion);
                                    else if (CompereNearMouse(tempTarget, minion) != null) tempTarget = CompereNearMouse(tempTarget, minion);
                                    else tempTarget = minion;                                 
                                }
                            }
                        return tempTarget;
                        }

                    default:
                        return null;
                }                
            }

            private static Obj_AI_Base GetLaneClearMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                Obj_AI_Base tempTarget = null;
                tempTarget = GetLastHitMinion(selectTime, Range, ExtendMonitarRange);

                if (tempTarget != null) return tempTarget;
                else if (ShouldWait(Range, ExtendMonitarRange)) return null;

                switch(selectTime)
                {
                    case MinionTeam.Enemy:

                        List<Obj_AI_Base> newTarget = new List<Obj_AI_Base>();

                        float predHealth = 0;
                        foreach (var minion in AllEnemyMinions(Range, ExtendMonitarRange))
                        {
                            predHealth = HealthPrediction.LaneClearHealthPrediction(minion, (int)((Self.AttackDelay * 1000) * 2f), _farmDelay);

                            if(predHealth >= 2 * Self.GetAutoAttackDamage(minion, true))
                                {
                                        newTarget.Add(minion);
                                }
                        }

                        foreach (var minion in newTarget)
                        {
                            if (tempTarget == null) tempTarget = minion;
                            else
                            {
                                if (ComperePriorety(tempTarget, minion) != null) tempTarget = ComperePriorety(tempTarget, minion);
                                else if (CompereMoreHealth(tempTarget, minion) != null) tempTarget = CompereMoreHealth(tempTarget, minion);
                                else if (CompereClosest(tempTarget, minion) != null) tempTarget = CompereClosest(tempTarget, minion);
                                else if (CompereNearMouse(tempTarget, minion) != null) tempTarget = CompereNearMouse(tempTarget, minion);
                                else tempTarget = minion;
                            }
                        }
                        return tempTarget;

                    case MinionTeam.Neutral:

                        foreach (var minion in AllNeutralMinions(Range, ExtendMonitarRange))
                            {
                                if (tempTarget == null) tempTarget = minion;
                                else tempTarget = CompereNeutralMonsterPriorety(tempTarget, minion);
                            }
                        return tempTarget;

                    default:
                        return null;
                }
            }

            private static Obj_AI_Base GetLaneFreezMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                Obj_AI_Base tempTarget = null;
                
                List<Obj_AI_Base> AllyMinions = new List<Obj_AI_Base>();
                List<Obj_AI_Base> EnemyMinions = new List<Obj_AI_Base>();

                foreach (var minion in AllAllyMinions(Range, ExtendMonitarRange)) AllyMinions.Add(minion);
                foreach (var minion in AllEnemyMinions(Range, ExtendMonitarRange)) EnemyMinions.Add(minion);

                if (AllyMinions.Count + 5 < EnemyMinions.Count) 
                {
                    tempTarget = GetLaneClearMinion(selectTime, Range, ExtendMonitarRange);
                }
                else
                {
                    tempTarget = GetLastHitMinion(selectTime, Range, ExtendMonitarRange);
                }
                return tempTarget;
            }

            private static Obj_AI_Base GetClosestMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                Obj_AI_Base newTarget = null;

                switch (selectTime)
                {
                    case MinionTeam.All:
                        foreach (var minion in AllMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Ally:
                        foreach (var minion in AllAllyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Enemy:
                        foreach (var minion in AllEnemyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Neutral:
                        foreach (var minion in AllNeutralMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    default:
                        return null;
                }
            }

            private static Obj_AI_Base GetFurthestMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                Obj_AI_Base newTarget = null;

                switch (selectTime)
                {
                    case MinionTeam.All:
                        foreach (var minion in AllMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereFurthest(newTarget, minion) != null) newTarget = CompereFurthest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Ally:
                        foreach (var minion in AllAllyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereFurthest(newTarget, minion) != null) newTarget = CompereFurthest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Enemy:
                        foreach (var minion in AllEnemyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereFurthest(newTarget, minion) != null) newTarget = CompereFurthest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Neutral:
                        foreach (var minion in AllNeutralMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereFurthest(newTarget, minion) != null) newTarget = CompereFurthest(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    default:
                        return null;
                }
            }

            private static Obj_AI_Base GetNearMouseMinion(MinionTeam selectTime, float Range, float ExtendMonitarRange = 0)
            {
                Obj_AI_Base newTarget = null;

                switch (selectTime)
                {
                    case MinionTeam.All:
                        foreach (var minion in AllMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Ally:
                        foreach (var minion in AllAllyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Enemy:                                        
                        foreach (var minion in AllEnemyMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    case MinionTeam.Neutral:
                        foreach (var minion in AllNeutralMinions(Range + ExtendMonitarRange))
                        {
                            if (newTarget == null) newTarget = minion;
                            else if (CompereNearMouse(newTarget, minion) != null) newTarget = CompereNearMouse(newTarget, minion);
                            else if (ComperePriorety(newTarget, minion) != null) newTarget = ComperePriorety(newTarget, minion);
                            else if (CompereLessHealth(newTarget, minion) != null) newTarget = CompereLessHealth(newTarget, minion);
                            else if (CompereClosest(newTarget, minion) != null) newTarget = CompereClosest(newTarget, minion);
                            else newTarget = minion;
                        }
                        return newTarget;
                    default:
                        return null;
                }
            }

            /// <summary>
            ///     Returns whether a minion will soon be for lasthit.
            /// </summary>
            public static bool ShouldWait(float Range, float ExtendMonitarRange = 0)
            {
                foreach (var minion in AllEnemyMinions(Range + ExtendMonitarRange))
                {
                    if (HealthPrediction.LaneClearHealthPrediction(minion, (int)((Self.AttackDelay * 1000) * 2f), _farmDelay) <= Self.GetAutoAttackDamage(minion,true))
                        return true;
                }
                return false;
            }

            static void Obj_AI_Turret_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                if (sender.IsValidTarget(1000, false, Self.Position) && sender.Name.Contains("Turret") && sender.IsAlly)
                {
                    float healthPred = HealthPrediction.GetHealthPrediction((Obj_AI_Base)args.Target, (int)((Self.AttackDelay * 1000) * 2f));
                    double turretDmg = sender.GetAutoAttackDamage((Obj_AI_Base)args.Target);
                    double selfAADmg = Self.GetAutoAttackDamage((Obj_AI_Base)args.Target, true);

                    if (healthPred <= selfAADmg)
                    {
                        _focustMinion = (Obj_AI_Base)args.Target;
                        return;
                    }
                    else if (turretDmg <= healthPred)
                    {
                        if (healthPred >= (2 * selfAADmg + turretDmg))
                        {
                            _focustMinion = (Obj_AI_Base)args.Target;
                            return;
                        }
                        else if (healthPred >= (selfAADmg + turretDmg + 5f))
                        {
                            return;
                        }
                        else
                        {
                            _focustMinion = null;
                            return;
                        }
                    }
                    else
                    {
                        _focustMinion = null;
                        return;
                    }
                }
                else _focustMinion = null;
            }

            public static void LastHitWithSpell(Spell spell, int minimumNumOfMinionsHit = 1)
            {

                if (spell.Type == SkillshotType.SkillshotLine)
                {
                    Obj_AI_Base minionToLastHit = null;
                    foreach (var minion in AllEnemyMinions(spell.Range))
                    {
                        var prediction = spell.GetPrediction(minion);

                        if (prediction.Hitchance == HitChance.High)
                        {
                            minionToLastHit = minion;
                            if (minion.Health > Self.GetAutoAttackDamage(minion, true) && spell.IsKillable(minion))
                                break;
                        }
                    }
                    if (minionToLastHit != null) spell.Cast(minionToLastHit);
                }
                else if (spell.Type == SkillshotType.SkillshotCircle)
                {
                    var farmLocation = MinionManager.GetBestCircularFarmLocation(AllEnemyMinions(spell.Range).Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width, spell.Range);

                    if (farmLocation.MinionsHit >= minimumNumOfMinionsHit)
                        spell.Cast(farmLocation.Position);
                }
                else if (spell.Type == SkillshotType.SkillshotCone)
                {
                    var farmLocation = MinionManager.GetBestCircularFarmLocation(AllEnemyMinions(spell.Range).Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width, spell.Range);

                    if (farmLocation.MinionsHit >= minimumNumOfMinionsHit)
                        spell.Cast(farmLocation.Position);
                }
                else if (spell.IsSkillshot == false)
                {
                    Obj_AI_Base minionToLastHit = null;
                    foreach(var minion in AllEnemyMinions(spell.Range))
                    {
                        minionToLastHit = minion;
                        if (minion.Health > Self.GetAutoAttackDamage(minion, true) && spell.IsKillable(minion))
                            break;
                    }
                    if (minionToLastHit != null) spell.Cast(minionToLastHit);
                }
                else return;
            }

            /// <summary>
            ///     Returns minion for the Range, SelectMode, ExtendMonitarRange, MinionTeam, FarmDelay. 
            ///     If you want to use this you will have to put it in OnGameUpdate becouse SMM dosent have any supscriptions to events.
            /// </summary>
            public static Obj_AI_Base GetTarget(float Range, MinionMode selectMode, float ExtendMonitarRange = 0, MinionTeam selectTime = MinionTeam.Enemy, int FarmDelay = 70)
            {
                _farmDelay = FarmDelay;

                switch (selectMode)
                {
                    case MinionMode.LastHit:
                        return GetLastHitMinion(selectTime, Range, ExtendMonitarRange);

                    case MinionMode.LaneClear:
                        return GetLaneClearMinion(selectTime, Range, ExtendMonitarRange);

                    case MinionMode.LaneFreez:
                        return GetLaneFreezMinion(selectTime, Range, ExtendMonitarRange);

                    case MinionMode.Closest:
                        return GetClosestMinion(selectTime, Range, ExtendMonitarRange);

                    case MinionMode.Furthest:
                        return GetFurthestMinion(selectTime, Range, ExtendMonitarRange);

                    case MinionMode.NearMouse:
                        return GetNearMouseMinion(selectTime, Range, ExtendMonitarRange);

                    default:
                        return null;
                }
            }

            public static void Target(float Range, MinionMode selectMode, float ExtendMonitarRange = 0, MinionTeam selectTime = MinionTeam.Enemy, int FarmDelay = 70)
            {
                _farmDelay = FarmDelay;

                if (_focustMinion != null)
                {
                    SelectedMinion = _focustMinion;
                    return;
                }

                switch (selectMode)
                {
                    case MinionMode.LastHit:
                        SelectedMinion = GetLastHitMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    case MinionMode.LaneClear:
                        SelectedMinion = GetLaneClearMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    case MinionMode.LaneFreez:
                        SelectedMinion = GetLaneFreezMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    case MinionMode.Closest:
                        SelectedMinion = GetClosestMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    case MinionMode.Furthest:
                        SelectedMinion = GetFurthestMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    case MinionMode.NearMouse:
                        SelectedMinion = GetNearMouseMinion(selectTime, Range, ExtendMonitarRange);
                        break;

                    default:
                        SelectedMinion = null;
                        break;
                }
            }
        }
    }
}
