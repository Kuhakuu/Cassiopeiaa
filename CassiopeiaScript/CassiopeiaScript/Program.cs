using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace CassiopeiaScript
{
    class Program
    {
        private static Spell.Skillshot Q, W, R;
        private static Spell.Targeted E, Ignite;
        private static int spellWidth, castDelay;
        private static Menu menu, comboMenu, harrasMenu, drawMenu, ksMenu, fleeMenu, laneclearMenu, junglaclearMenu;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Cassiopeia")
            {
                return;
            }

            LoadSpells();
            LoadMenu();
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += KillSteal;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;

        }

        private static void LoadSpells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 1, null, 130);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 250, 3000, 180);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Targeted(SpellSlot.E, 700);

            R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, spellWidth = 90, castDelay = 500);
            R.AllowedCollisionCount = int.MaxValue;

            Ignite = new Spell.Targeted(Player.Instance.GetSpellSlotFromName("Ignite"), 600);
        }

        private static void LoadMenu()
        {
            menu = MainMenu.AddMenu("Cassiopeia", "Cassiopeia");
            menu.AddSeparator();

            comboMenu = menu.AddSubMenu("Combo", "Combo");
            comboMenu.AddSeparator();

            comboMenu.AddLabel("Combo Configs");
            comboMenu.Add("useQ", new CheckBox("Use Q in Combo"));
            comboMenu.Add("useW", new CheckBox("Use W in Combo"));
            comboMenu.Add("useE", new CheckBox("Use E in Combo"));
            comboMenu.Add("useR", new CheckBox("Use R in Combo"));
            comboMenu.AddSeparator();
            comboMenu.Add("useIGNITE", new CheckBox("Use IGNITE in Combo"));
            comboMenu.Add("AntiGap", new CheckBox("Use R to ANTIGAPCLOSER"));
            comboMenu.Add("useAuto", new CheckBox("Disable AA in Combo"));
            comboMenu.AddSeparator();
            comboMenu.Add("Elasthit", new CheckBox("Use E to LastHit"));

            harrasMenu = menu.AddSubMenu("Harras", "Harras");
            harrasMenu.AddSeparator();

            harrasMenu.AddLabel("Harras Configs");
            harrasMenu.Add("harrasQ", new CheckBox("Use Q in Harras"));
            harrasMenu.Add("AutoQ", new CheckBox("Toggle Q"));

            laneclearMenu = menu.AddSubMenu("LaneClear");
            laneclearMenu.AddSeparator();

            laneclearMenu.AddLabel("LaneClear Configs");
            laneclearMenu.Add("laneclearQ", new CheckBox("Use Q in LaneClear"));
            laneclearMenu.Add("laneclearW", new CheckBox("Use W in LaneClear"));
            laneclearMenu.Add("laneclearE", new CheckBox("Use E in LaneClear"));

            junglaclearMenu = menu.AddSubMenu("JunglaClear");
            junglaclearMenu.AddSeparator();

            junglaclearMenu.Add("jcQ", new CheckBox("Use Q in JunglaClear"));
            junglaclearMenu.Add("jcW", new CheckBox("Use W in JunglaClear"));
            junglaclearMenu.Add("jcE", new CheckBox("Use E in JunglaClear"));

            drawMenu = menu.AddSubMenu("Drawings", "Drawings");
            drawMenu.AddSeparator();

            drawMenu.AddLabel("Drawings Configs");
            drawMenu.Add("drawQ", new CheckBox("Draw Q"));
            drawMenu.Add("drawW", new CheckBox("Draw W"));
            drawMenu.Add("drawE", new CheckBox("Draw E"));
            drawMenu.Add("drawR", new CheckBox("Draw R"));

            ksMenu = menu.AddSubMenu("KillSteal", "KillSteal");
            ksMenu.AddSeparator();

            ksMenu.Add("ksQ", new CheckBox("Use Q for KS"));
            ksMenu.Add("ksW", new CheckBox("Use W for KS"));
            ksMenu.Add("ksE", new CheckBox("Use E for KS"));
            ksMenu.Add("ksR", new CheckBox("Use R for KS"));

            fleeMenu = menu.AddSubMenu("Flee", "Flee");
            fleeMenu.AddSeparator();

            fleeMenu.Add("fleeQ", new CheckBox("Use Q in FleeMode"));
            fleeMenu.Add("fleeW", new CheckBox("Use W in FleeMode"));

        }

        private static void Game_OnTick(EventArgs args)
        {
            AutoQ();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harras();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHitQ();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JunglaClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (drawMenu["drawQ"].Cast <CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Green, Q.Range, Player.Instance.Position);
            }

            if (drawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Green, W.Range, Player.Instance.Position);
            }

            if (drawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Green, E.Range, Player.Instance.Position);
            }

            if (drawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Green, R.Range, Player.Instance.Position);
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if ((target == null) || target.IsInvulnerable)
            {
                return;
            }

            var poison = EntityManager.Heroes.Enemies.Find(e => e.IsValidTarget(E.Range) && e.HasBuffOfType(BuffType.Poison));

            if (comboMenu["useAuto"].Cast<CheckBox>().CurrentValue)
            {
                Orbwalker.DisableAttacking = true;

                var useQ = comboMenu["useQ"].Cast<CheckBox>().CurrentValue;
                if (useQ && Q.IsInRange(target) && Q.IsReady() && !target.IsInvulnerable)
                {
                    Q.Cast(target);
                }

                var useW = comboMenu["useW"].Cast<CheckBox>().CurrentValue;
                if (useW && W.IsInRange(target) && W.IsReady() && !target.IsInvulnerable)
                {
                    W.Cast(target);
                }

                var useE = comboMenu["useE"].Cast<CheckBox>().CurrentValue;
                if (useE && E.IsInRange(target) && E.IsReady() && !target.IsInvulnerable)
                {
                    if (Q.IsOnCooldown && poison.IsValidTarget(E.Range))
                    {
                        E.Cast(target);
                    }
                    else
                    {
                        E.Cast(target);
                    }
                }

                var useR = comboMenu["useR"].Cast<CheckBox>().CurrentValue;
                if (useR && R.IsReady() && R.IsInRange(target) && !target.IsInvulnerable &&
                    target.IsFacing(ObjectManager.Player))
                {
                    R.Cast(target);
                }
            }
            else
            {
                Orbwalker.DisableAttacking = false;

                var useQ = comboMenu["useQ"].Cast<CheckBox>().CurrentValue;
                if (useQ && Q.IsInRange(target) && Q.IsReady() && !target.IsInvulnerable)
                {
                    Q.Cast(target);
                }

                var useW = comboMenu["useW"].Cast<CheckBox>().CurrentValue;
                if (useW && W.IsInRange(target) && W.IsReady() && !target.IsInvulnerable)
                {
                    W.Cast(target);
                }

                var useE = comboMenu["useE"].Cast<CheckBox>().CurrentValue;
                if (useE && E.IsInRange(target) && E.IsReady() && !target.IsInvulnerable)
                {
                    if (Q.IsOnCooldown && poison.IsValidTarget(E.Range))
                    {
                        E.Cast(target);
                    }
                    else
                    {
                        E.Cast(target);
                    }
                }

                var useR = comboMenu["useR"].Cast<CheckBox>().CurrentValue;
                if (useR && R.IsReady() && R.IsInRange(target) && !target.IsInvulnerable && target.IsFacing(ObjectManager.Player))
                {
                    R.Cast(target);
                }
            }
        }

        private static void Harras()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || target.IsInvulnerable)
            {
                return;
            }

            var useQ = harrasMenu["harrasQ"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsInRange(target) && Q.IsReady() && !target.IsInvulnerable)
            {
                Q.Cast(target);
            }
        }

        private static void AutoQ()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || target.IsInvulnerable)
            {
                return;
            }

            var useQ = harrasMenu["AutoQ"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsInRange(target) && Q.IsReady() && !target.IsInvulnerable)
            {
                Q.Cast(target);
            }
        }

        private static void LastHitQ()
        {
            foreach (
                var min in
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(a => a.Health > 0 && a.IsValidTarget(E.Range)))
            {
                if (comboMenu["Elasthit"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                    Player.Instance.GetSpellDamage(min, SpellSlot.E) > min.TotalShieldHealth())
                {
                    E.Cast(min);
                }
            }
        }

        private static void LaneClear()
        {
            foreach (
                var enemy in
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(a => a.Health > 0 && a.IsValidTarget(Q.Range)))
            {
                if (laneclearMenu["laneclearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
                    Q.IsInRange(enemy))
                {
                    Q.Cast(enemy);
                }

                if (laneclearMenu["laneclearW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    W.IsInRange(enemy))
                {
                    W.Cast(enemy);
                }

                if (laneclearMenu["laneclearE"].Cast<CheckBox>().CurrentValue && E.IsReady() &&
                    E.IsInRange(enemy))
                {
                    E.Cast(enemy);
                }
            }
        }

        private static void JunglaClear()
        {
            foreach (
                var enemy in
                    EntityManager.MinionsAndMonsters.GetJungleMonsters()
                        .Where(a => a.Health > 0 && a.IsValidTarget(Q.Range)))
            {
                if (junglaclearMenu["jcQ"].Cast<CheckBox>().CurrentValue && Q.IsInRange(enemy) &&
                    Q.IsReady())
                {
                    Q.Cast(enemy);
                }

                if (junglaclearMenu["jcW"].Cast<CheckBox>().CurrentValue && W.IsInRange(enemy) &&
                    W.IsReady())
                {
                    W.Cast(enemy);
                }

                if (junglaclearMenu["jcE"].Cast<CheckBox>().CurrentValue && E.IsInRange(enemy) &&
                    E.IsReady())
                {
                    E.Cast(enemy);
                }
            }
        }

        private static bool IsKilleable(AIHeroClient target , SpellSlot spell )
        {
            var fullHealth = target.TotalShieldHealth();

            if (target.HasUndyingBuff() || target.HasBuff("BardRStasis"))
            {
                return false;
            }

            return (Player.Instance.GetSpellDamage(target, spell) >= fullHealth);
        }

        private static void KillSteal(EventArgs args)
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsZombie && a.Health > 0))
            {
                if (ksMenu["ksQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(enemy) && 
                    IsKilleable(enemy, Q.Slot))
                {
                    Q.Cast(enemy);
                }

                if (ksMenu["ksW"].Cast<CheckBox>().CurrentValue && W.IsReady() && W.IsInRange(enemy) &&
                    IsKilleable(enemy, W.Slot))
                {
                    W.Cast(enemy);
                }

                if (ksMenu["ksE"].Cast<CheckBox>().CurrentValue && E.IsReady() && E.IsInRange(enemy) &&
                    IsKilleable(enemy, E.Slot))
                {
                    E.Cast(enemy);
                }

                if (ksMenu["ksR"].Cast<CheckBox>().CurrentValue && R.IsInRange(enemy) && R.IsReady() &&
                    IsKilleable(enemy, R.Slot))
                {
                    R.Cast(enemy);
                }

                if (comboMenu["useIGNITE"].Cast<CheckBox>().CurrentValue && Ignite.IsReady() && Player.Instance.CountEnemyChampionsInRange(600) >= 1
                    && IsKilleable(enemy, Ignite.Slot))
                {
                    Ignite.Cast(enemy);
                }
            }
        }

        private static void Gapcloser_OnGapcloser(Obj_AI_Base sender, EventArgs args)
        {
            if (sender.IsEnemy && sender is AIHeroClient && sender.Distance(ObjectManager.Player) < R.Range &&
                R.IsReady() && comboMenu["AntiGap"].Cast<CheckBox>().CurrentValue)
            {
                R.Cast(sender);
            }
        }

        private static void Flee()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null || target.IsInvulnerable)
            {
                return;
            }

            if (fleeMenu["fleeQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(target) 
                && !target.IsInvulnerable && target.IsValidTarget())
            {
                Q.Cast(target);
            }

            if (fleeMenu["fleeW"].Cast<CheckBox>().CurrentValue && W.IsReady() && W.IsInRange(target) 
                && !target.IsInvulnerable && target.IsValidTarget())
            {
                W.Cast(target);
            }
        }
    }
}
