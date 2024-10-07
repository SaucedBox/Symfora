using System;
using System.Collections.Generic;
using System.Linq;
using TripleS.UI;
using Microsoft.Xna.Framework;
using TripleS;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input;

namespace Symfora {
    public static class UIDesigner {

        public static string PreviousMenu { get; private set; }
        public static string CurrentMenu { get; private set; }
        public static string PreviousPauseMenu { get; private set; }

        private static int bossHealth;
        private static int bossMaxHealth;
        private static string bossName;
        private static int[] bossBarCutoffs;
        private static QuickTimer mTransTimer;
        private static bool transitioning;
        private static int currentSetting;
        private static int currentStat;
        private static int fakeStatPoints;
        private static bool binding;
        private static bool levelTrans = true;
        public static int upgradePoints;
        private static bool changingStats;
        private static int selectedUpSlot;
        private static bool openedSaveSlots;
        private static int selectedSave;

        public static void Load(bool refresh)
        {
            UIManager.Reset();
            if (!refresh)
            {
                PreviousMenu = "";
                CurrentMenu = "";
                PreviousPauseMenu = "";
            }

            UIManager.AddFont("regular", "font/MainFont", SSS.Game.Content, -1f);
            UIManager.AddFont("bold", "font/MainFont_bold", SSS.Game.Content, -1f);
            UIManager.AddFont("italic", "font/MainFont_italy", SSS.Game.Content, -1f);
            UIManager.AddFont("small", "font/MainFont_small", SSS.Game.Content, -1f);
            Element element;

            #region Settings
            element = StyledElement(0, "settings_back", "");
            element.Color = Color.Black;
            element.Width = Core._Renderer.View.windowWidth;
            element.Height = Core._Renderer.View.windowHeight;
            element.Sizing = SizingStyle.Sretch;
            element.Image = SSS.Square;
            UIManager.AddElement(element);

            element = StyledElement(0, "settings_title", "italic");
            element.Position = new Vector2(8, 8);
            element.Conents = LocalizationManager.UINames["menuSetting"];
            UIManager.AddElement(element);

            element = StyledElement(2, "settings_apply", "bold");
            element.Position = new Vector2(8, 32);
            element.Conents = LocalizationManager.UINames["settingButApply"];
            UIManager.AddElement(element);

            for (int i = 0; i < Core._Settings.Count; i++)
            {
                element = StyledElement(4, $"settings_s{i}", "small");
                element.Position += new Vector2(0, 64 + (11 * i));
                element.Conents = LocalizationManager.UINames[$"setting{i}"];
                UIManager.AddElement(element);
                element = StyledElement(4, $"settings_l{i}", "small");
                element.Position += new Vector2(74, 64 + (11 * i));
                element.Conents = "N/A";
                UIManager.AddElement(element);
            }

            int qi = -1;
            foreach (string name in GameInputs.Controls.Keys)
            {
                qi++;
                element = StyledElement(4, $"settings_k{qi}", "small");
                element.Position += new Vector2(0, 74 + (Core._Settings.Count * 11) + (11 * qi));
                element.Conents = LocalizationManager.UINames[$"settingK{name}"];
                UIManager.AddElement(element);
                element = StyledElement(4, $"settings_kl{qi}", "small");
                element.Position += new Vector2(74, 64 + ((Core._Settings.Count + 1) * 11) + (11 * qi));
                element.Conents = $"{GameInputs.Controls[name]}";
                UIManager.AddElement(element);
            }

            element = StyledElement(2, "settings_butChangeL", "small");
            element.Conents = "<";
            UIManager.AddElement(element);
            element = StyledElement(2, "settings_butChangeR", "small");
            element.Conents = ">";
            UIManager.AddElement(element);
            element = StyledElement(2, "settings_butChangeKey", "small");
            element.Conents = "[BIND]";
            UIManager.AddElement(element);
            #endregion

            #region Misc
            element = StyledElement(1, "misc_transition", "");
            element.Color = Color.Black;
            element.Width = Core._Renderer.View.windowWidth;
            element.Height = Core._Renderer.View.windowHeight;
            element.Sizing = SizingStyle.Sretch;
            element.TrueVisible = true;
            element.Visible = true;
            element.Alpha = 0;
            element.Image = SSS.Square;
            UIManager.AddElement(element);
            #endregion

            if (!LevelHandler.MainMenu)
            {
                #region Game
                element = StyledElement(1, "game_weapon", "");
                element.Anchor = AnchorPoint.TopLeft;
                element.Position = new Vector2(8, 8);
                UIManager.AddElement(element);

                element = StyledElement(1, "game_boosts", "");
                element.Position = new Vector2(11.75f, 32);
                element.Image = AssetManager.GetAsset("ui_boost");
                UIManager.AddElement(element);

                element = StyledElement(1, "game_health", "");
                element.Position = new Vector2(32, 18);
                element.Image = AssetManager.GetAsset("ui_health");
                UIManager.AddElement(element);

                element = StyledElement(1, "game_boss", "");
                element.Position = new Vector2(-22, 8);
                element.Anchor = AnchorPoint.TopRight;
                element.Image = AssetManager.GetAsset("ui_boss");
                UIManager.AddElement(element);

                element = StyledElement(1, "game_boss_name", "regular");
                element.Anchor = AnchorPoint.TopRight;
                element.Color = Color.Transparent;
                UIManager.AddElement(element);

                UIManager.AddElement(element);

                element = StyledElement(1, "game_jet", "");
                element.Height = 9;
                element.Sizing = SizingStyle.Sretch;
                element.Visible = true;
                UIManager.AddElement(element);

                element = StyledElement(0, "game_interact", "regular");
                element.Anchor = AnchorPoint.Middle;
                element.Position = new Vector2(0, Core._Player.Transform.Height / 2 * Core._Renderer.View.Zoom);
                element.Conents = LocalizationManager.GetVariableString("gameInteraction", true, GameInputs.Controls["interact"].ToString());
                element.Visible = false;
                UIManager.AddElement(element);
                #endregion

                #region Pause
                element = StyledElement(0, "pause_back", "");
                element.Color = Color.Black * 0.25f;
                element.Width = Core._Renderer.View.windowWidth;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_title", "italic");
                element.Position = new Vector2(8, 8);
                element.Conents = LocalizationManager.UINames[Core._LevelHandler.levName];
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_prog", "regular");
                element.Position = new Vector2(8, 22);
                element.Conents = (Core.Progress * 10).ToString() + "%";
                UIManager.AddElement(element);

                element = StyledElement(2, "pause_butResume", "bold");
                element.Position = new Vector2(8, 50);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["pauseButResume"];
                UIManager.AddElement(element);

                element = StyledElement(2, "pause_butSettings", "bold");
                element.Position = new Vector2(8, 70);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["menuSetting"];
                UIManager.AddElement(element);

                element = StyledElement(2, "pause_butQuit", "bold");
                element.Position = new Vector2(8, 90);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["pauseButTitle"];
                UIManager.AddElement(element);

                element = StyledElement(2, "pause_butExit", "bold");
                element.Position = new Vector2(8, 110);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["menuButExit"];
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charLine1", "");
                element.Position = new Vector2(-100, 0);
                element.Width = 5;
                element.Color = Color.White;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                element.Anchor = AnchorPoint.TopRight;
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charLine2", "");
                element.Position = new Vector2(-200, 0);
                element.Width = 5;
                element.Color = Color.White;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                element.Anchor = AnchorPoint.TopRight;
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charStatTitle", "italic");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = LocalizationManager.UINames["charStatTitle"];
                element.Position = new Vector2(-194, 4);
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charStatPoints", "small");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = PhantomManager.StatPoints + " " + LocalizationManager.UINames["charStatPoints"];
                element.Position = new Vector2(-194, 22);
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charUpgradeTitle", "italic");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = LocalizationManager.UINames["menuUpgrade"];
                element.Position = new Vector2(-94, 4);
                UIManager.AddElement(element);

                var stats = GetStats();

                element = StyledElement(0, "pause_charStatLabels", "small");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = stats[0];
                element.Position = new Vector2(-196, 42);
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charStatValues", "small");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = stats[1];
                element.Position = new Vector2(-125, 42);
                UIManager.AddElement(element);

                if (LevelHandler.CurrentLevel > 1 && LevelHandler.CurrentLevel < 20)
                {
                    element = StyledElement(0, "pause_charPhantomPoints", "small");
                    element.Anchor = AnchorPoint.TopRight;
                    int currentTallyID = PhantomManager.GetCurrentTallyID();
                    element.Conents = PhantomManager.Tallies[currentTallyID].Count(x => x) + "/" + PhantomManager.Tallies[currentTallyID].Length + " " + LocalizationManager.UINames["upgradePoints"];
                    element.Position = new Vector2(-94, 22);
                    UIManager.AddElement(element);
                }

                element = StyledElement(0, "pause_charUpgradeTitle", "italic");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = LocalizationManager.UINames["menuUpgrade"];
                element.Position = new Vector2(-94, 4);
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charUpgradeEq", "regular");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = LocalizationManager.UINames["charUpgradeEquiped"];
                element.Position = new Vector2(-96, 42);
                UIManager.AddElement(element);

                element = StyledElement(0, "pause_charUpgradeUn", "regular");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = LocalizationManager.UINames["charUpgradeUnlocked"];
                element.Position = new Vector2(-96, 76);
                UIManager.AddElement(element);

                string upgrades = "";
                int count = Core._Player.Upgrades.Count(x => x == Symfora.Upgrade.None);
                if (count == 1)
                {
                    int index = (int)Core._Player.Upgrades.Where(x => x != Symfora.Upgrade.None).First();
                    upgrades = LocalizationManager.UINames["upgrade" + (index - 1)];
                }
                else if (count == 0)
                {
                    int index = (int)Core._Player.Upgrades[0] - 1;
                    upgrades = LocalizationManager.UINames["upgrade" + index];
                    index = (int)Core._Player.Upgrades[1] - 1;
                    upgrades += "\n" + LocalizationManager.UINames["upgrade" + index];
                }

                element = StyledElement(0, "pause_charUpgradeEqList", "small");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = upgrades;
                element.Position = new Vector2(-96, 56);
                UIManager.AddElement(element);

                string alreadys = "";
                for (int i = 0; i < Enum.GetNames<Upgrade>().Length - 1; i++)
                {
                    if (GameStateManager.States.ContainsKey(48) && GameStateManager.States[48 + i] == 0)
                        alreadys += LocalizationManager.UINames["upgrade" + i] + '\n';
                }

                element = StyledElement(0, "pause_charUpgradeUnList", "small");
                element.Anchor = AnchorPoint.TopRight;
                element.Conents = alreadys;
                element.Position = new Vector2(-96, 88);
                UIManager.AddElement(element);
                #endregion

                #region Death
                element = StyledElement(0, "death_back", "");
                element.Color = Color.Black;
                element.Width = Core._Renderer.View.windowWidth;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);
                #endregion

                #region Upgrade Menu
                element = StyledElement(0, "upgrade_back", "");
                element.Color = Color.Black;
                element.Width = Core._Renderer.View.windowWidth;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "upgrade_title", "italic");
                element.Position = new Vector2(8, 8);
                element.Conents = LocalizationManager.UINames["menuUpgrade"];
                UIManager.AddElement(element);

                var currentTallyAr = PhantomManager.Tallies[PhantomManager.LastSoanoHeadTallyID];

                element = StyledElement(0, "upgrade_points", "small");
                element.Position = new Vector2(86, 16);
                element.Conents = upgradePoints + "/" + currentTallyAr.Length + " " + LocalizationManager.UINames["upgradePoints"];
                UIManager.AddElement(element);

                element = StyledElement(0, "upgrade_selectedTitle", "bold");
                element.Position = new Vector2(86, 36);
                UIManager.AddElement(element);

                element = StyledElement(0, "upgrade_selectedDesc", "small");
                element.Position = new Vector2(86, 51);
                UIManager.AddElement(element);

                int trueIndex = 0;
                float alph = CurrentMenu == "upgrade" ? 1 : 0;
                alreadys = "";
                for (int i = 0; i < Enum.GetNames<Upgrade>().Length - 1; i++)
                {
                    if (GameStateManager.States.ContainsKey(48) && GameStateManager.States[48 + i] == -1)
                    {
                        element = StyledElement(3, "upgrade_upg" + i, "small");
                        element.Conents = LocalizationManager.UINames["upgrade" + i];
                        element.Position = new Vector2(8, 36 + (14 * (i - trueIndex)));
                        element.Alpha = alph;
                        UIManager.AddElement(element);
                    }
                    else
                    {
                        trueIndex++;
                        alreadys += LocalizationManager.UINames["upgrade" + i] + '\n';
                    }
                }

                element = StyledElement(0, "upgrade_aqUpTitle", "regular");
                element.Position = new Vector2(86, 80);
                element.Conents = LocalizationManager.UINames["upgradeCurrent"];
                element.Visible = alreadys != "";
                element.Alpha = alph;
                UIManager.AddElement(element);

                element = StyledElement(0, "upgrade_aqUp", "small");
                element.Position = new Vector2(86, 96);
                element.Conents = alreadys;
                element.Alpha = alph;
                UIManager.AddElement(element);
                #endregion

                #region Character
                element = StyledElement(0, "char_back", "");
                element.Color = Color.Black;
                element.Width = Core._Renderer.View.windowWidth;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "char_title", "italic");
                element.Position = new Vector2(8, 8);
                element.Conents = LocalizationManager.UINames["charTitle"];
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statPoints", "small");
                element.Conents = PhantomManager.StatPoints + " " + LocalizationManager.UINames["charStatPoints"];
                element.Position = new Vector2(8, 28);
                UIManager.AddElement(element);

                element = StyledElement(0, "char_line1", "");
                element.Position = new Vector2(0, 46);
                element.Width = Core._Renderer.View.windowWidth;
                element.Color = Color.White;
                element.Height = 5;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                float line2X = Core._Renderer.View.windowWidth / 4 / 3;
                element = StyledElement(0, "char_line2", "");
                element.Position = new Vector2(line2X, 46);
                element.Width = 5;
                element.Color = Color.White;
                element.Height = Core._Renderer.View.windowHeight - 46;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statTitle", "italic");
                element.Position = new Vector2(4, 50);
                element.Conents = LocalizationManager.UINames["charStatTitle"];
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statTooltip", "small");
                element.Position = new Vector2(4, 68);
                element.Conents = LocalizationManager.UINames["charStatTut"];
                UIManager.AddElement(element);

                element = StyledElement(0, "char_upgradeTitle", "italic");
                element.Position = new Vector2(line2X + 8, 50);
                element.Conents = LocalizationManager.UINames["menuUpgrade"];
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statTooltip", "small");
                element.Position = new Vector2(line2X + 8, 68);
                element.Conents = LocalizationManager.UINames["charUpgradeTut"];
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statLabels", "small");
                element.Conents = stats[0];
                element.Position = new Vector2(4, 113);
                UIManager.AddElement(element);

                element = StyledElement(0, "char_statValues", "small");
                element.Conents = stats[1];
                element.Position = new Vector2(72, 113);
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butStats", "bold");
                element.Position = new Vector2(4, 90);
                element.Width = 50;
                element.Height = 20;
                if (PhantomManager.StatPoints <= 0)
                {
                    element.TextColor = Color.Gray;
                    element.Conents = LocalizationManager.UINames["charChangeStats2"];
                }
                else
                    element.Conents = LocalizationManager.UINames["charChangeStats0"];
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butChangeL", "small");
                element.Conents = "<";
                element.Visible = false;
                UIManager.AddElement(element);
                element = StyledElement(2, "char_butChangeR", "small");
                element.Conents = ">";
                element.Visible = false;
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butResetStats", "bold");
                element.Position = new Vector2(4, 200);
                element.Width = 50;
                element.Height = 20;
                element.Visible = GameStateManager.States.ContainsKey(66) && GameStateManager.States[66] > 0;
                element.Conents = LocalizationManager.UINames["charResetStats"];
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butEquipUpgrade0", "bold");
                element.Position = new Vector2(line2X + 4, 80);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["upgrade" + ((int)Core._Player.Upgrades[0] - 1)];
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butEquipUpgrade1", "bold");
                element.Position = new Vector2(line2X + 4, 98);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["upgrade" + ((int)Core._Player.Upgrades[1] - 1)];
                UIManager.AddElement(element);

                for (int i = 0; i < Enum.GetNames<Upgrade>().Length - 1; i++)
                {
                    if (GameStateManager.States.ContainsKey(48) && GameStateManager.States[48 + i] == 0)
                    {
                        element = StyledElement(2, "char_upg" + i, "small");
                        element.Conents = LocalizationManager.UINames["upgrade" + i];
                        element.Position = new Vector2(line2X + 4, 110 + (12 * i));
                        UIManager.AddElement(element);
                    }
                }

                element = StyledElement(0, "char_selectedTitle", "bold");
                element.Position = new Vector2(line2X + 90, 119);
                UIManager.AddElement(element);

                element = StyledElement(0, "char_selectedDesc", "small");
                element.Position = new Vector2(line2X + 90, 134);
                UIManager.AddElement(element);

                element = StyledElement(2, "char_butApply", "bold");
                element.Position = new Vector2(4, -10);
                element.Anchor = AnchorPoint.BottomLeft;
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["charApply"];
                UIManager.AddElement(element);
                #endregion

                #region Library
                element = StyledElement(0, "library_back", "");
                element.Color = Color.Black;
                element.Width = Core._Renderer.View.windowWidth;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "library_title", "italic");
                element.Position = new Vector2(8, 8);
                element.Conents = LocalizationManager.UINames["libraryTitle"];
                UIManager.AddElement(element);

                int tomeCount = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (GameStateManager.States.ContainsKey(67 + i) && GameStateManager.States[67 + i] == 1)
                    {
                        element = StyledElement(2, "library_butTome" + i, "small");
                        element.Conents = LocalizationManager.Dialogues["book" + i + "Title"];
                        element.Position = new Vector2(4, 55 + (12 * tomeCount));
                        UIManager.AddElement(element);
                        tomeCount++;
                    }
                }

                element = StyledElement(0, "library_tally", "small");
                element.Position = new Vector2(4, 30);
                element.Conents = tomeCount + "/10 " + LocalizationManager.UINames["libraryTally"] + "\n" + LocalizationManager.UINames["libraryTip"];
                UIManager.AddElement(element);

                line2X = Core._Renderer.View.windowWidth / 4 / 6;
                element = StyledElement(0, "library_line", "");
                element.Position = new Vector2(line2X, 0);
                element.Width = 5;
                element.Color = Color.White;
                element.Height = Core._Renderer.View.windowHeight;
                element.Sizing = SizingStyle.Sretch;
                element.Image = SSS.Square;
                UIManager.AddElement(element);

                element = StyledElement(0, "library_textTitle", "italic");
                element.Position = new Vector2(line2X + 8, 8);
                UIManager.AddElement(element);

                element = StyledElement(0, "library_textSubtitle", "small");
                element.Position = new Vector2(line2X + 8, 25);
                UIManager.AddElement(element);

                element = StyledElement(0, "library_text", "small");
                element.Position = new Vector2(line2X + 8, 55);
                UIManager.AddElement(element);

                element = StyledElement(2, "library_butBack", "bold");
                element.Position = new Vector2(4, -10);
                element.Anchor = AnchorPoint.BottomLeft;
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["libraryEscape"];
                UIManager.AddElement(element);
                #endregion
            }
            else
            {
                #region Main Menu
                string slotBrac = $" ({LocalizationManager.UINames["mainmenuSlot"]} {selectedSave + 1})";
                element = StyledElement(2, "mainMenu_butContinue", "bold");
                element.Position = new Vector2(4, 50);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["mainmenuContinue"] + slotBrac;
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_butWipe", "bold");
                element.Position = new Vector2(4, 70);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["mainmenuWipe"] + slotBrac;
                element.Visible = GameStateManager.SaveBeenWiped();
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_butLoad", "bold");
                element.Position = new Vector2(4, 100);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["mainmenuSaveLoad"];
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_butSettings", "bold");
                element.Position = new Vector2(4, 120);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["menuSetting"];
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_butExit", "bold");
                element.Position = new Vector2(4, 140);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["mainmenuExit"];
                UIManager.AddElement(element);

                for (int i = 0; i < 3; i++)
                {
                    element = StyledElement(2, "mainMenu_butLoadSlot" + i, "regular");
                    element.Position = new Vector2(50, 100);
                    element.Width = 50;
                    element.Height = 20;
                    element.Alpha = 0;
                    element.Conents = LocalizationManager.UINames["mainmenuSlot"] + " " + (i + 1);
                    UIManager.AddElement(element);
                }

                element = StyledElement(0, "mainMenu_Copyright", "small");
                element.Position = new Vector2(-170, -12);
                element.Anchor = AnchorPoint.BottomRight;
                element.Conents = LocalizationManager.UINames["mainmenuCopyright"];
                UIManager.AddElement(element);

                element = StyledElement(0, "mainMenu_wipeText", "regular");
                element.Position = new Vector2(4, 50);
                element.Conents = LocalizationManager.UINames["mainmenuWipeConfirm"];
                element.Visible = false;
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_wipeYes", "bold");
                element.Position = new Vector2(4, 70);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["generalYes"];
                element.Visible = false;
                UIManager.AddElement(element);

                element = StyledElement(2, "mainMenu_wipeNo", "bold");
                element.Position = new Vector2(50, 70);
                element.Width = 50;
                element.Height = 20;
                element.Conents = LocalizationManager.UINames["generalNo"];
                element.Visible = false;
                UIManager.AddElement(element);
                #endregion
            }
        }

        public static void Update(GameTime time)
        {
            UIManager.Update(time, Core._Renderer.View.MasterMatrix);
            if (transitioning)
            {
                float timerVal = mTransTimer.UpdateFloat(time);
                transitioning = timerVal < 1f;
                foreach (Element el in UIManager.elements.Where(x => x.MenuID == PreviousMenu))
                {
                    if(transitioning)
                        el.Alpha = MathF.Abs(timerVal - 1f);
                    else
                    {
                        el.Alpha = 1;
                        el.TrueVisible = false;
                    }
                }
                foreach (Element el in UIManager.elements.Where(x => x.MenuID == CurrentMenu))
                {
                    if (transitioning)
                        el.Alpha = timerVal;
                    else
                        el.Alpha = 1;
                }
            }

            var lTransBack = UIManager.GetElement("misc_transition");
            if (!levelTrans && lTransBack.Alpha < 1)
                lTransBack.Alpha += SSS.Delta * 2;
            if (levelTrans && lTransBack.Alpha > 0)
                lTransBack.Alpha -= SSS.Delta * 2;

            if (CurrentMenu == "settings" && !binding)
            {
                var rp = MathF.Round((GameInputs.GetMouse().Y / 4 - 63) / 11) * 11;

                if (rp < 55)
                {
                    rp = Math.Clamp(rp, 0, 11 * (Core._Settings.Count - 1));
                    currentSetting = (int)(rp / 11);
                    UIManager.GetElement("settings_butChangeL").Position = new Vector2(116, rp + 63);
                    UIManager.GetElement("settings_butChangeR").Position = new Vector2(130, rp + 63);
                }
                else
                {
                    rp = Math.Clamp(rp - 55, 0, 11 * (GameInputs.Controls.Count - 1)) + 55;
                    currentSetting = (int)((rp - 55) / 11);
                    UIManager.GetElement("settings_butChangeKey").Position = new Vector2(116, rp + 63);
                }
            }
            if (binding && GameInputs.LastKey(out Keys res))
            {
                binding = false;
                UIManager.blockButtons = false;
                var thingy = GameInputs.Controls.ToList()[currentSetting].Key;

                GameInputs.Controls[thingy] = res;
                UIManager.GetElement("settings_butChangeKey").Conents = "[BIND]";
                UIManager.GetElement("settings_kl" + currentSetting).Conents = res.ToString();
            }

            if(CurrentMenu == "upgrade")
            {
                foreach(Element button in UIManager.elements.Where(x => x.ID.Remove(x.ID.Length - 1) == "upgrade_upg"))
                {
                    if (button.State == ElementState.Highlighted)
                    {
                        int upID = int.Parse(button.ID[^1].ToString());
                        UIManager.GetElement("upgrade_selectedTitle").Conents = LocalizationManager.UINames["upgrade" + upID];
                        UIManager.GetElement("upgrade_selectedDesc").Conents = LocalizationManager.UINames["upgrade" + upID + "Desc"];
                        break;
                    }
                }
            }

            if(CurrentMenu == "char")
            {
                if (changingStats)
                {
                    var rp = MathF.Round((GameInputs.GetMouse().Y / 4 - 110) / 7.5f) * 7.5f;
                    rp = (rp / 7.5f) - 1;
                    int sid = (int)rp;
                    if (rp >= 4)
                    {
                        sid--;
                        if (rp == 4)
                            rp = 3;
                    }
                    if (rp >= 8)
                    {
                        sid--;
                        if (rp == 8)
                            rp = 7;
                    }
                    sid = Math.Clamp(sid, 0, 7);
                    rp = Math.Clamp(rp, 0, 9);

                    UIManager.GetElement("char_butChangeR").Position = new Vector2(112, 113 + (rp * 7.5f) - 2);
                    UIManager.GetElement("char_butChangeL").Position = new Vector2(100, 113 + (rp * 7.5f) - 2);
                    currentStat = sid;
                }
                else
                {
                    foreach (Element button in UIManager.elements.Where(x => x.ID.Remove(x.ID.Length - 1) == "char_upg"))
                    {
                        if (button.State == ElementState.Highlighted)
                        {
                            int upID = int.Parse(button.ID[^1].ToString());
                            UIManager.GetElement("char_selectedTitle").Conents = LocalizationManager.UINames["upgrade" + upID];
                            UIManager.GetElement("char_selectedDesc").Conents = LocalizationManager.UINames["upgrade" + upID + "Desc"];
                            break;
                        }
                    }
                }
            }

            if(CurrentMenu == "mainMenu")
            {
                if (openedSaveSlots)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var element = UIManager.GetElement("mainMenu_butLoadSlot" + i);
                        element.Visible = true;
                        var x = MathHelper.Lerp(element.Position.X, 65 + (40 * i), 0.1f);
                        element.Position = new Vector2(x, 100);
                    }

                    foreach(Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && !x.ID.Contains("Load")))
                    {
                        if(ele.State == ElementState.Highlighted)
                        {
                            openedSaveSlots = false;
                            var bEle = (ButtonElement)UIManager.GetElement("mainMenu_butLoad");
                            bEle.ChangeTextColor(Color.White);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var element = UIManager.GetElement("mainMenu_butLoadSlot" + i);
                        element.Visible = false;
                        element.Position = new Vector2(50, 100);
                    }
                }
            }
        }

        public static void Draw(Renderer renderer)
        {
            if (!LevelHandler.MainMenu)
            {
                UIManager.sizer.view = renderer.View;
                var element = UIManager.GetElement("game_boosts");
                var oldPos = element.Position;
                if (!Core._Player.BlockSelfBoost)
                {
                    for (int i = 0; i < Core._Player.MaxSelfBoosts; i++)
                    {
                        element.Position += new Vector2(0, i * 14);
                        element.Color = i > Core._Player.SelfBoosts - 1 ? Color.Black : Color.White;
                        if (Core._Player.SelfBoosts < Core._Player.MaxSelfBoosts && i == Core._Player.SelfBoosts)
                        {
                            element.Image = AssetManager.GetAsset("ui_boostDead");
                            float am = Core._Player.sbRegenTimer / 5f;
                            element.Color = new Color(am, am, am);
                        }
                        else
                            element.Image = AssetManager.GetAsset("ui_boost");
                        UIManager.DrawIndElement(element, renderer, true);
                        element.Position = oldPos;
                    }
                }

                element = UIManager.GetElement("game_health");
                oldPos = element.Position;
                for (int i = 0; i < Core._Player.MaxHealth; i++)
                {
                    element.Position += new Vector2(i * 9, 0);
                    element.Color = i > Core._Player.Health - 1 ? Color.Black : Color.White;
                    UIManager.DrawIndElement(element, renderer, true);
                    element.Position = oldPos;
                }

                if (Core._Player.MaxJetTime > 0)
                {
                    element = UIManager.GetElement("game_jet");
                    element.Sizing = SizingStyle.Anchor;

                    element.Image = AssetManager.GetAsset("ui_jbStart");
                    element.Position = new Vector2(32, 8);
                    UIManager.DrawIndElement(element, renderer, true);
                    element.Image = AssetManager.GetAsset("ui_jbEnd");
                    element.Position = new Vector2(33.5f + (Core._Player.MaxJetTime * 12), 8);
                    UIManager.DrawIndElement(element, renderer, true);

                    element.Image = AssetManager.GetAsset("ui_jbMid1");
                    element.Height = element.Image.Height * (int)element.Scale;
                    element.Position = new Vector2(33.5f, 8);
                    element.Sizing = SizingStyle.Sretch;
                    element.SubLayer = 10;
                    element.Width = (int)(Core._Player.MaxJetTime * 12 * (element.Scale + 1f));
                    UIManager.DrawIndElement(element, renderer, true);

                    element.Image = AssetManager.GetAsset("ui_jbMid");
                    element.SubLayer = 0;
                    if (Core._Player.JetBoosted)
                        element.Width = (int)(Core._Player.jetTimer * 12 * (element.Scale + 1f));
                    else
                        element.Width = 0;
                }

                element = UIManager.GetElement("game_boss");
                if (bossHealth > 0)
                {
                    oldPos = element.Position;
                    for (int i = 0; i < bossMaxHealth; i++)
                    {
                        if (bossBarCutoffs != null && bossBarCutoffs.Length > 0)
                        {
                            int phase = 0;
                            int amount = 0;
                            for (int di = 0; di < bossBarCutoffs.Length; di++) 
                            {
                                if (i < bossBarCutoffs[di])
                                    phase++;
                            }

                            if (phase < bossBarCutoffs.Length)
                            {
                                if(phase == 0)
                                    amount = bossBarCutoffs.Max();
                                else if(phase == 1)
                                    amount = bossBarCutoffs.Min();
                            }

                            var col = i < bossHealth ? Color.White : Color.Black;
                            element.Color = col;
                            element.Position -= new Vector2((i - amount) * 14, -14 * phase);
                            UIManager.DrawIndElement(element, renderer, true);
                            element.Position = oldPos;
                        }
                        else
                        {
                            var col = i < bossHealth ? Color.White : Color.Black;
                            element.Color = col;
                            element.Position -= new Vector2(i * 14, 0);
                            UIManager.DrawIndElement(element, renderer, true);
                            element.Position = oldPos;
                        }
                    }

                    /*element = UIManager.GetElement("game_boss_name");
                    element.Visible = true;
                    element.TextSize = 2;
                    element.Conents = bossName;
                    var sb = UIManager.fonts["regular"].MeasureString(bossName);
                    element.Position = oldPos - new Vector2((sb.X / 2) - 10, 2 - ((bossBarCutoffs.Length + 1) * 14));*/
                }
                else
                {
                    element.Visible = false;
                }

                bossHealth = -1;
            }
        }

        public static void ChangeMenu(string menu, bool trans = true)
        {
            PreviousMenu = CurrentMenu;
            CurrentMenu = menu;
            if (trans)
            {
                transitioning = true;
                mTransTimer ??= new QuickTimer(0, 1f, true, 2f);
                mTransTimer.Reset();
            }
            foreach (Element el in UIManager.elements.Where(x => x.MenuID == PreviousMenu))
            {
                if (trans)
                    el.Alpha = 1;
                else
                    el.TrueVisible = false;
            }
            foreach (Element el in UIManager.elements.Where(x => x.MenuID == CurrentMenu))
            {
                if (trans)
                    el.Alpha = 0;
                else
                    el.Alpha = 1;
                el.TrueVisible = true;
            }

            if (menu == "game")
                ChangeUI(0);
            else if (menu == "settings")
                ChangeUI(3);
        }

        private static Element StyledElement(int style, string name, string font)
        {
            Element element = new Element(name);
            element.Style = style;
            if(element.MenuID != CurrentMenu)
                element.TrueVisible = false;
            if(font != "")
                element.Font = font;
            switch (style)
            {
                case 0:
                    element.Sizing = SizingStyle.Anchor;
                    element.Color = Color.Transparent;
                    element.Scale = 1;
                    element.Visible = true;
                    break;
                case 1:
                    element.Scale = 3;
                    element.Anchor = AnchorPoint.TopLeft;
                    element.Color = Color.White;
                    element.Sizing = SizingStyle.Anchor;
                    element.Alpha = 1;
                    break;
                case 2:
                    element = new ButtonElement(name, Color.Black * 0.5f, Color.White, Color.White * 0.5f, Color.White, Color.Black);
                    if (font != "")
                        element.Font = font;
                    element.Sizing = SizingStyle.Sretch;
                    element.TextSize = 1f;
                    element.OnClick += ButtonClicks;
                    element.Padding = 3;
                    element.TextAlignment = TextPosition.Middle;
                    element.Visible = true;
                    break;
                case 3:
                    element = new ButtonElement(name, Color.Black * 0.5f, Color.White, Color.White * 0.5f, Color.White, Color.Black);
                    element.Font = font;
                    element.Sizing = SizingStyle.Sretch;
                    element.TextSize = 1f;
                    element.OnClick += Upgrade;
                    element.Padding = 3;
                    element.TextAlignment = TextPosition.Middle;
                    element.Visible = true;
                    break;
                case 4:
                    element.Color = Color.Transparent;
                    element.Sizing = SizingStyle.Anchor;
                    element.Scale = 1;
                    element.Position = new Vector2(8, 0);
                    element.Visible = true;
                    break;
            }
            return element;
        }

        //lmaoooo
        public static void ChangeUI(int changeID)
        {
            switch (changeID) 
            {
                case 0:
                    if (CurrentMenu == "game")
                    {
                        var ceel = UIManager.GetElement("game_weapon");
                        ceel.Visible = true;
                        ceel.Image = AssetManager.GetAsset("ui_weapon" + Core._Player.WeaponType.ToString());
                    }
                    break;
                case 2:
                    if (CurrentMenu == "pause")
                        PreviousPauseMenu = PreviousMenu;
                    break;
                case 3:
                    UIManager.GetElement("settings_l0").Conents = Core._Settings[0] == 1 ? "On" : "Off";
                    UIManager.GetElement("settings_l1").Conents = $"{Core._Settings[1] + 1}/3";
                    UIManager.GetElement("settings_l2").Conents = Core._Settings[2] == 1 ? "On" : "Off";
                    UIManager.GetElement("settings_l3").Conents = $"{Core._Settings[3] + 1}/3";
                    break;
                case 4:
                    var element = UIManager.GetElement("game_interact");
                    element.Visible = !element.Visible;
                    break;
                case 5:
                    levelTrans = false;
                    UIManager.GetElement("misc_transition").Alpha = 0;
                    break;
                case 6:
                    levelTrans = true;
                    UIManager.GetElement("misc_transition").Alpha = 1;
                    break;
            }
        }

        static Dictionary<int, int> changedStats;

        private static void ButtonClicks(object sender, ButtonEventArgs args)
        {
            int max = currentSetting == 1 ? 2 : 1;
            switch (args.ElName)
            {
                case "pause_butExit":
                    Core.ExitGame();
                    break;
                case "pause_butResume":
                    ChangeMenu(PreviousPauseMenu);
                    Core.Paused = false;
                    break;
                case "pause_butSettings":
                    ChangeMenu("settings");
                    Core.SetSettings(true);
                    ChangeUI(3);
                    break;
                case "pause_butQuit":
                    Core.LoadLevel(1);
                    break;
                case "settings_apply":
                    ChangeMenu(LevelHandler.MainMenu ? "mainMenu" : "pause");
                    Core.SetSettings(false);
                    break;
                case "settings_butChangeL":
                    Core._Settings[currentSetting] = Math.Clamp(Core._Settings[currentSetting] - 1, 0, max);
                    ChangeUI(3);
                    break;
                case "settings_butChangeR":
                    Core._Settings[currentSetting] = Math.Clamp(Core._Settings[currentSetting] + 1, 0, max);
                    ChangeUI(3);
                    break;
                case "settings_butChangeKey":
                    binding = true;
                    UIManager.GetElement("settings_butChangeKey").Conents = LocalizationManager.UINames["settingKeybind"];
                    UIManager.blockButtons = true;
                    break;
                case "char_butStats":
                    if (PhantomManager.StatPoints > 0)
                    {
                        changingStats = !changingStats;
                        if (changingStats)
                        {
                            UIManager.GetElement("char_butStats").Conents = LocalizationManager.UINames["charChangeStats1"];
                            UIManager.GetElement("char_butChangeR").Visible = true;
                            UIManager.GetElement("char_butChangeL").Visible = true;
                            UIManager.GetElement("char_butResetStats").Visible = false;
                            fakeStatPoints = PhantomManager.StatPoints;
                            changedStats = new Dictionary<int, int>();
                        }
                        else
                        {
                            int locID = fakeStatPoints <= 0 ? 2 : 0;
                            var element = UIManager.GetElement("char_butStats");
                            element.Conents = LocalizationManager.UINames["charChangeStats" + locID];
                            if (locID == 2)
                            {
                                var b = (ButtonElement)element;
                                b.ChangeTextColor(Color.Gray);
                            }
                            UIManager.GetElement("char_butChangeR").Visible = false;
                            UIManager.GetElement("char_butChangeL").Visible = false;
                            UIManager.GetElement("char_butResetStats").Visible = true;
                            PhantomManager.StatPoints = fakeStatPoints;
                        }
                    }
                    break;
                case "char_butChangeR":
                    if (fakeStatPoints > 0)
                    {
                        fakeStatPoints--;
                        if (!changedStats.ContainsKey(currentStat))
                            changedStats.Add(currentStat, Core.GetStat(currentStat));
                        ChangePlayerStat(true, currentStat);
                        UIManager.GetElement("char_statValues").Conents = GetStats()[1];
                        UIManager.GetElement("char_statPoints").Conents = fakeStatPoints + " " + LocalizationManager.UINames["charStatPoints"];
                    }
                    break;
                case "char_butChangeL":
                    if (fakeStatPoints < PhantomManager.StatPoints && changedStats.ContainsKey(currentStat) && changedStats[currentSetting] < Core.GetStat(currentStat))
                    {
                        fakeStatPoints++;
                        ChangePlayerStat(false, currentStat);
                        UIManager.GetElement("char_statValues").Conents = GetStats()[1];
                        UIManager.GetElement("char_statPoints").Conents = fakeStatPoints + " " + LocalizationManager.UINames["charStatPoints"];
                    }
                    break;
                case "char_butResetStats":
                    if (GameStateManager.States[66] > 0 && !changingStats)
                    {
                        PhantomManager.StatPoints = (int)GameStateManager.States[66];
                        Core._Player.ResetStats();
                        UIManager.GetElement("char_butResetStats").Visible = false;
                        UIManager.GetElement("char_statValues").Conents = GetStats()[1];
                        UIManager.GetElement("char_statPoints").Conents = PhantomManager.StatPoints + " " + LocalizationManager.UINames["charStatPoints"];
                        UIManager.GetElement("char_butStats").Conents = LocalizationManager.UINames["charChangeStats0"];
                    }
                    break;
                case "char_butEquipUpgrade0":
                    if (!changingStats)
                    {
                        selectedUpSlot = 0;
                        var b = (ButtonElement)UIManager.GetElement("char_butEquipUpgrade1");
                        b.ChangeTextColor(Color.Gray);
                        b = (ButtonElement)UIManager.GetElement("char_butEquipUpgrade0");
                        b.ChangeTextColor(Color.White);
                    }
                    break;
                case "char_butEquipUpgrade1":
                    if (!changingStats)
                    {
                        selectedUpSlot = 1;
                        var b = (ButtonElement)UIManager.GetElement("char_butEquipUpgrade1");
                        b.ChangeTextColor(Color.White);
                        b = (ButtonElement)UIManager.GetElement("char_butEquipUpgrade0");
                        b.ChangeTextColor(Color.Gray);
                    }
                    break;
                case "char_butApply":
                    if (!changingStats)
                    {
                        fakeStatPoints = 0;
                        changingStats = false;
                        selectedUpSlot = 0;
                        upgradePoints = 0;
                        GameStateManager.States[36] = (int)Core._Player.Upgrades[0];
                        GameStateManager.States[37] = (int)Core._Player.Upgrades[1];
                        Core._Player.Frozen = false;
                        Core._Player.Health = Core._Player.MaxHealth;
                        ChangeMenu("game");
                    }
                    break;
                case "library_butBack":
                    Core._Player.Frozen = false;
                    ChangeMenu("game");
                    break;
                case "mainMenu_butExit":
                    Core.ExitGame();
                    break;
                case "mainMenu_butSettings":
                    ChangeMenu("settings");
                    Core.SetSettings(true);
                    ChangeUI(3);
                    break;
                case "mainMenu_butLoad":
                    openedSaveSlots = true;
                    var bEle = (ButtonElement)UIManager.GetElement("mainMenu_butLoad");
                    bEle.ChangeTextColor(new Color(0.25f, 0.25f, 0.25f));
                    break;
                case "mainMenu_butWipe":
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("but")))
                        ele.Visible = false;
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("wipe")))
                        ele.Visible = true;
                    break;
                case "mainMenu_wipeNo":
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("but")))
                        ele.Visible = true;
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("wipe")))
                        ele.Visible = false;
                    break;
                case "mainMenu_wipeYes":
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("but")))
                        ele.Visible = true;
                    foreach (Element ele in UIManager.elements.Where(x => x.MenuID == "mainMenu" && x.ID.Contains("wipe")))
                        ele.Visible = false;
                    SSS.ClearSave();
                    UIManager.GetElement("mainMenu_butWipe").Visible = false;
                    break;
                case "mainMenu_butContinue":
                    GameStateManager.ReadSave();
                    if (GameStateManager.States.ContainsKey(1))
                        Core.LoadLevel((int)GameStateManager.States[1]);
                    else
                        Core.LoadLevel(5);
                    break;
            }
            if (args.ElName.Contains("char_upg"))
            {
                if (!changingStats)
                {
                    int upID = int.Parse(args.ElName[^1].ToString()) + 1;
                    if (!Core._Player.Upgrades.Contains((Upgrade)upID))
                    {
                        Core._Player.Upgrades[selectedUpSlot] = (Upgrade)upID;
                        UIManager.GetElement("char_butEquipUpgrade" + selectedUpSlot).Conents = LocalizationManager.UINames["upgrade" + (upID - 1)];
                    }
                }
            }                      
            if (args.ElName.Contains("library_butTome"))
            {
                int bookID = int.Parse(args.ElName[^1].ToString());
                UIManager.GetElement("library_textTitle").Conents = LocalizationManager.Dialogues["book" + bookID + "Title"];
                UIManager.GetElement("library_textSubtitle").Conents = LocalizationManager.Dialogues["book" + bookID + "Subtitle"];

                var element = UIManager.GetElement("library_text");
                string main = LocalizationManager.Dialogues["book" + bookID + "Contents"];
                var lines = main.Split(@"\n");
                string trueText = "";

                for (int li = 0; li < lines.Length; li++)
                {
                    string l = lines[li];
                    int boundsWidth = Core._Renderer.View.windowWidth - (Core._Renderer.View.windowWidth / 4 / 6);
                    boundsWidth -= (int)(boundsWidth * 0.25f);

                    float textScale = (UIManager.scale + element.Scale) / element.TextSize;
                    int seperands = (int)MathF.Ceiling((UIManager.fonts["small"].MeasureString(l).X * textScale) / boundsWidth) - 1;

                    if (seperands > 0)
                    {
                        int charWidth = l.Length / (seperands + 1);
                        for (int i = 0; i < seperands; i++)
                        {
                            int index = lines[li].IndexOf(' ', charWidth * (i + 1));
                            lines[li] = lines[li].Insert(index, Environment.NewLine);
                        }
                    }

                    trueText += lines[li] + "\n";
                }
                element.Conents = trueText;
            }
            if (args.ElName.Contains("mainMenu_butLoadSlot"))
            {
                selectedSave = int.Parse(args.ElName[^1].ToString());

                string slotBrac = $" ({LocalizationManager.UINames["mainmenuSlot"]} {selectedSave + 1})";
                var element = UIManager.GetElement("mainMenu_butContinue");
                element.Conents = LocalizationManager.UINames["mainmenuContinue"] + slotBrac;
                element = UIManager.GetElement("mainMenu_butWipe");
                element.Visible = GameStateManager.SaveBeenWiped();
                element.Conents = LocalizationManager.UINames["mainmenuWipe"] + slotBrac;
                GameStateManager.CurrentSave = selectedSave;
            }
        }

        private static void Upgrade(object sender, ButtonEventArgs args)
        {
            int upID = int.Parse(args.ElName[^1].ToString());
            GameStateManager.States[upID + 48] = 0;
            upgradePoints -= 1;
            if (upgradePoints <= 0)
                ChangeMenu("game");
            else
                Load(true);
        }

        private static void ChangePlayerStat(bool adder, int sid)
        {
            int value = adder ? 1 : -1;
            switch (sid)
            {
                case 0: Core._Player.MaxHealth += value; 
                    break;
                case 1: Core._Player.MaxBoosts += value; 
                    break;
                case 2: Core._Player.MaxSelfBoosts += value; 
                    break;
                case 3: Core._Player.MaxJetTime += value; 
                    break;
                case 4: Core._Player.Speed += 1 / (5 / 2.5f) * value;
                    break;
                case 5: Core._Player.JumpVel += 1 / (5 / 1.8f) * value;
                    break;
                case 6: Core._Player.RamAmount += 1 / (5 / 8.5f) * value;
                    break;
                case 7: Core._Player.KnockbackRes += 1 / 5 * value;
                    break;
            }
        }

        private static string[] GetStats()
        {
            string labels = "", values = "";
            for (int i = 0; i < 8; i++)
            {
                if (i == 4 || i == 7)
                {
                    labels += "\n";
                    values += "\n";
                }
                labels += LocalizationManager.UINames["charStat" + i] + "\n";
                string extra = i == 3 ? " " + LocalizationManager.UINames["charSeconds"] : "";
                values += Core.GetStat(i) + extra + "\n";
            }
            return new string[2] { labels, values };
        }

        public static void SetBossInfo(int maxH, int h, int[] phases, string nameId)
        {
            bossBarCutoffs = phases;
            bossHealth = h;
            bossMaxHealth = maxH;
            bossName = LocalizationManager.UINames[nameId];
        }
    }
}
