﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NSMBe4 {
    public partial class DataFinder : Form {
        public DataFinder(NitroClass ROM) {
            this.ROM = ROM;
            InitializeComponent();

            string[] LevelNames;
            if (Properties.Settings.Default.Language != 1) {
                LevelNames = NSMBe4.Properties.Resources.levelnames.Split('\n');
            } else {
                LevelNames = NSMBe4.Properties.Resources.levelnames_lang1.Split('\n');
            }

            Levels = new List<string>();
            LevelFiles = new List<string>();

            string WorldID = null;
            for (int NameIdx = 0; NameIdx < LevelNames.Length; NameIdx++) {
                LevelNames[NameIdx] = LevelNames[NameIdx].Trim();
                if (LevelNames[NameIdx] == "") continue;
                if (LevelNames[NameIdx][0] == '-') {
                    string[] ParseWorld = LevelNames[NameIdx].Substring(1).Split('|');
                    WorldID = ParseWorld[1];
                } else {
                    string[] ParseLevel = LevelNames[NameIdx].Split('|');
                    if (ParseLevel[2] == "1") {
                        Levels.Add(ParseLevel[0]);
                        LevelFiles.Add(WorldID + ParseLevel[1] + "_1.bin");
                    } else {
                        // Create a subfolder
                        int AreaCount = int.Parse(ParseLevel[2]);
                        for (int AreaIdx = 1; AreaIdx <= AreaCount; AreaIdx++) {
                            Levels.Add(ParseLevel[0] + " area " + AreaIdx.ToString());
                            LevelFiles.Add(WorldID + ParseLevel[1] + "_" + AreaIdx.ToString() + ".bin");
                        }
                    }
                }
            }
        }

        private NitroClass ROM;
        private List<string> Levels;
        private List<string> LevelFiles;

        private void DataFinder_Load(object sender, EventArgs e) {
            if (Properties.Settings.Default.Language == 1) {
                Text = "Buscador de Data";

                label1.Text = "Esta herramienta no es util para casi todos. Pero si estas tratando de buscar algo especifico (por ejemplo, como el data para un sprite trabaja) puedes usarlo para coger una lista de todas las vezes que esta usado - para que sepas donde mirar.";
                findBlockRadioButton.Text = "Buscar todos del bloque";
                findSpriteRadioButton.Text = "Buscar todos del sprite";
                label2.Text = "partido en:";
                processButton.Text = "Buscar!";
            }
        }

        private void processButton_Click(object sender, EventArgs e) {
            if (!findBlockRadioButton.Checked && !findSpriteRadioButton.Checked) {
                if (Properties.Settings.Default.Language != 1) {
                    MessageBox.Show("Choose a mode to search in.");
                } else {
                    MessageBox.Show("Escoger un modo para buscar.");
                }
                return;
            }

            StringBuilder output = new StringBuilder();

            if (findBlockRadioButton.Checked) {
                if (Properties.Settings.Default.Language != 1) {
                    output.AppendLine("-- All instances of block " + blockNumberUpDown.Value.ToString() + " in levels: --");
                } else {
                    output.AppendLine("-- Todas las instancias de bloque " + blockNumberUpDown.Value.ToString() + " en niveles: --");
                }

                int BlockToDump = (int)((blockNumberUpDown.Value - 1) * 8);
                int SplitVal = (int)(splitCountUpDown.Value);

                for (int LevelIdx = 0; LevelIdx < Levels.Count; LevelIdx++) {
                    byte[] CurrentLevel = ROM.ExtractFile(ROM.FileIDs[LevelFiles[LevelIdx]]);
                    int BlockOffset = (int)(CurrentLevel[BlockToDump] | (CurrentLevel[BlockToDump + 1] << 8) | (CurrentLevel[BlockToDump + 2] << 16) | (CurrentLevel[BlockToDump + 3] << 24));
                    int BlockSize = (int)(CurrentLevel[BlockToDump + 4] | (CurrentLevel[BlockToDump + 5] << 8) | (CurrentLevel[BlockToDump + 6] << 16) | (CurrentLevel[BlockToDump + 7] << 24));
                    if (SplitVal > 0) {
                        int SplitCount = (int)Math.Ceiling((double)BlockSize / SplitVal);
                        int SplitOffset = 0;
                        //System.Diagnostics.Debug.Print("BlockSize: " + BlockSize.ToString());
                        //System.Diagnostics.Debug.Print("SplitVal: " + SplitVal.ToString());
                        //System.Diagnostics.Debug.Print("SplitCount: " + SplitCount.ToString());
                        for (int SplitIdx = 0; SplitIdx < SplitCount; SplitIdx++) {
                            if (labellingTypeCheckBox.Checked) {
                                output.Append(LevelFiles[LevelIdx]);
                            } else {
                                output.Append(Levels[LevelIdx]);
                            }
                            output.Append(": ");
                            //System.Diagnostics.Debug.Print("Printing: BlockOffset = {0:D}, SplitOffset = {1:D}, BlockSize = {2:D}, start = {3:D}, end = {4:D}", BlockOffset, SplitOffset, BlockSize, BlockOffset + SplitOffset, BlockOffset + Math.Min(SplitVal, BlockSize));
                            PrintByteArray(output, CurrentLevel, BlockOffset + SplitOffset, BlockOffset + Math.Min(SplitOffset + SplitVal, BlockSize));
                            SplitOffset += SplitVal;
                            output.Append("\r\n");
                        }
                        output.Append("\r\n");
                    } else {
                        if (labellingTypeCheckBox.Checked) {
                            output.Append(LevelFiles[LevelIdx]);
                        } else {
                            output.Append(Levels[LevelIdx]);
                        }
                        output.Append(": ");
                        PrintByteArray(output, CurrentLevel, BlockOffset, BlockOffset + BlockSize);
                        output.Append("\r\n");
                    }
                }
            } else if (findSpriteRadioButton.Checked) {
                if (Properties.Settings.Default.Language != 1) {
                    output.AppendLine("-- All instances of sprite " + spriteUpDown.Value.ToString() + " in levels: --");
                } else {
                    output.AppendLine("-- Todas las instancias de sprite " + spriteUpDown.Value.ToString() + " en niveles: --");
                }

                for (int LevelIdx = 0; LevelIdx < Levels.Count; LevelIdx++) {
                    byte[] CurrentLevel = ROM.ExtractFile(ROM.FileIDs[LevelFiles[LevelIdx]]);
                    int SpriteOffset = CurrentLevel[0x30] | (CurrentLevel[0x31] << 8) | (CurrentLevel[0x32] << 16) | (CurrentLevel[0x33] << 24);
                    int SpriteSize = CurrentLevel[0x34] | (CurrentLevel[0x35] << 8) | (CurrentLevel[0x36] << 16) | (CurrentLevel[0x37] << 24);
                    byte[] SpriteBlock = new byte[SpriteSize];
                    Array.Copy(CurrentLevel, SpriteOffset, SpriteBlock, 0, SpriteSize);

                    int SpriteCount = (SpriteSize - 4) / 12;
                    int FilePos = 0;
                    for (int SpriteIdx = 0; SpriteIdx < SpriteCount; SpriteIdx++) {
                        int SpriteType = SpriteBlock[FilePos] | (SpriteBlock[FilePos + 1] << 8);
                        if (SpriteType == spriteUpDown.Value) {
                            if (labellingTypeCheckBox.Checked) {
                                output.Append(LevelFiles[LevelIdx]);
                            } else {
                                output.Append(Levels[LevelIdx]);
                            }
                            output.Append(": ");
                            PrintByteArray(output, SpriteBlock, FilePos + 6, FilePos + 12);
                            output.Append("\r\n");
                        }
                        FilePos += 12;
                    }
                }
            }

            outputTextBox.Text = output.ToString();
        }

        private void PrintByteArray(StringBuilder sb, byte[] array, int start, int end) {
            bool space = false;
            for (int idx = start; idx < end; idx++) {
                sb.Append(String.Format("{0:X2}", array[idx]));
                if (space) sb.Append(' ');
                space = !space;
            }
        }
    }
}