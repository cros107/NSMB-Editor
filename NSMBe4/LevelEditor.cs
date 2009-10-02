﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NSMBe4 {
    public partial class LevelEditor : Form {

        public ObjectsEditionMode oem;
        public EntrancesEditionMode eem;
        public PathsEditionMode pem;
        public ViewsEditionMode vem, zem;
        public ObjectPickerControl opc;

        public ToolsForm tools;
        private List<ToolStripButton> EditionModeButtons;

        public LevelEditor(NitroClass ROM, string LevelFilename) {
            InitializeComponent();
            this.ROM = ROM;
            this.LevelFilename = LevelFilename;
            editObjectsButton.Checked = true;

            smallBlockOverlaysToolStripMenuItem.Checked = Properties.Settings.Default.SmallBlockOverlays;

            EditionModeButtons = new List<ToolStripButton>();
            EditionModeButtons.Add(editObjectsButton);
            EditionModeButtons.Add(editEntrancesButton);
            EditionModeButtons.Add(editPathsButton);
            EditionModeButtons.Add(editViewsButton);
            EditionModeButtons.Add(editZonesButton);

            if (Properties.Settings.Default.Language == 1)
            {
                saveLevelButton.Text = "Guardar Nivel";
                viewMinimapButton.Text = "Ver Mapa";
                levelConfigButton.Text = "Configuracion de Nivel";
                toolStripLabel1.Text = "Edicion de:";
                editObjectsButton.Text = "Objetos/Sprites";
                editEntrancesButton.Text = "Entradas";
                editPathsButton.Text = "Rutas";
                optionsMenu.Text = "Opciones";
                smallBlockOverlaysToolStripMenuItem.Text = "Ver contenidos de bloque en pequeño";
                deleteAllObjectsToolStripMenuItem.Text = "Borrar todos los objetos";
                deleteAllSpritesToolStripMenuItem.Text = "Borrar todos los sprites";

            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            //ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;

            // First off prepare the sprite list
            string[] spritelist = new string[324];
            string[] rawlist;
            if (Properties.Settings.Default.Language != 1) {
                rawlist = Properties.Resources.spritelist.Split('\n');
            } else {
                rawlist = Properties.Resources.spritelist_lang1.Split('\n');
            }
            foreach (string sprite in rawlist) {
                string trimmedsprite = sprite.Trim();
                if (trimmedsprite == "") continue;
                int equalPos = trimmedsprite.IndexOf('=');
                spritelist[int.Parse(trimmedsprite.Substring(0, equalPos))] = trimmedsprite.Substring(0, equalPos) + ": " + trimmedsprite.Substring(equalPos + 1);
            }

            ushort LevelFileID = ROM.FileIDs[LevelFilename + ".bin"];
            ushort LevelBGDatFileID = ROM.FileIDs[LevelFilename + "_bgdat.bin"];

            // There's a catch 22 here: Level loading requires graphics. Graphics loading requires level.
            // Therefore, I have a simple loader here which gets this info.
            byte[] LevelFile = ROM.ExtractFile(LevelFileID);
            int Block1Offset = LevelFile[0] | (LevelFile[1] << 8) | (LevelFile[2] << 16) | (LevelFile[3] << 24);
            int Block4Offset = LevelFile[12] | (LevelFile[13] << 8) | (LevelFile[14] << 16) | (LevelFile[15] << 24);
            byte TilesetID = LevelFile[Block1Offset + 0x0C];
            byte TilesetPalID = LevelFile[Block4Offset + 3];

            GFX = new NSMBGraphics(ROM);
            GFX.LoadTilesets(TilesetID, TilesetPalID);

            Level = new NSMBLevel(ROM, LevelFileID, LevelBGDatFileID, GFX);
            levelEditorControl1.Initialise(GFX, Level, this);

            opc = new ObjectPickerControl();
            opc.Initialise(GFX);
            oem = new ObjectsEditionMode(Level, levelEditorControl1, opc);
            eem = new EntrancesEditionMode(Level, levelEditorControl1);
            pem = new PathsEditionMode(Level, levelEditorControl1);
            vem = new ViewsEditionMode(Level, levelEditorControl1, true);
            zem = new ViewsEditionMode(Level, levelEditorControl1, false);

            levelEditorControl1.SetEditionMode(oem);

            tools = new ToolsForm(levelEditorControl1);
            MinimapForm = new LevelMinimap(Level, levelEditorControl1);
            levelEditorControl1.minimap = MinimapForm;
            MinimapForm.Text = "Minimap - " + this.Text;
        }

        private void viewMap16ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Map16ViewerForm == null || Map16ViewerForm.IsDisposed) {
                Map16ViewerForm = new Map16Viewer(GFX);
            }
            Map16ViewerForm.Show();
        }

        private Map16Viewer Map16ViewerForm;
        private LevelMinimap MinimapForm;
        private LevelConfig LevelConfigForm;
        private NSMBLevel Level;
        private NSMBGraphics GFX;
        private NitroClass ROM;

        public string LevelFilename;

        private bool Dirty;

        private UserControl SelectedPanel;

        public void SetPanel(UserControl np)
        {
            if (SelectedPanel == np) return;

            if(SelectedPanel != null)
                SelectedPanel.Parent = null;
            np.Size = PanelContainer.Size;
            np.Location = new Point(0, 0);
            np.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            SelectedPanel = np;
            if (SelectedPanel != null)
                SelectedPanel.Parent = PanelContainer;
        }

        public bool ForceClose() {
            if (Dirty) {
                DialogResult dr;
                if (Properties.Settings.Default.Language != 1) {
                    dr = MessageBox.Show("This level contains unsaved changes.\nIf you close the editor without saving, you will lose them.\nDo you want to save?", "NSMB Editor 4", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                } else {
                    dr = MessageBox.Show("Este nivel tiene cambios sin guardar.\nSi cierras el editor sin guardar, los perderas.\nQuieres guardarlos?", "NSMB Editor 4", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                }
                if (dr == DialogResult.Yes) {
                    Level.Save();
                } else if (dr == DialogResult.Cancel) {
                    return true;
                }
            }

            return false;
        }

        private void saveLevelButton_Click(object sender, EventArgs e) {
            Dirty = false;
            Level.Save();
        }

        private void LevelEditor_FormClosing(object sender, FormClosingEventArgs e) {
            if (ForceClose()) {
             //   e.Cancel = true;
            }
        }

        private void viewMinimapButton_Click(object sender, EventArgs e)
        {
            MinimapForm.Show();
        }

        private void levelConfigButton_Click(object sender, EventArgs e) {
            if (LevelConfigForm == null || LevelConfigForm.IsDisposed) {
                LevelConfigForm = new LevelConfig(Level, ROM);
            }
            LevelConfigForm.SetDirtyFlag += new LevelConfig.SetDirtyFlagDelegate(levelEditorControl1_SetDirtyFlag);
            LevelConfigForm.ReloadTileset += new LevelConfig.ReloadTilesetDelegate(LevelConfigForm_ReloadTileset);
            LevelConfigForm.RefreshMainWindow += new LevelConfig.RefreshMainWindowDelegate(LevelConfigForm_RefreshMainWindow);
            LevelConfigForm.LoadSettings();
            LevelConfigForm.Show();
        }

        private void LevelConfigForm_ReloadTileset() {
            GFX.LoadTileset1(Level.Blocks[0][0xC], Level.Blocks[3][3]);
            Level.ReRenderAll();
            opc.ReRenderAll(1);
            Invalidate(true);
        }

        private void LevelConfigForm_RefreshMainWindow() {
            Invalidate(true);
        }

        private void LevelEditor_FormClosed(object sender, FormClosedEventArgs e) {
            if (Map16ViewerForm != null) {
                Map16ViewerForm.Close();
            }
            if (MinimapForm != null) {
                MinimapForm.Close();
            }
            if (LevelConfigForm != null) {
                LevelConfigForm.Close();
            }
            if (tools != null)
                tools.Close();
        }

        private void levelEditorControl1_SetDirtyFlag() {
            Dirty = true;
        }

        private void smallBlockOverlaysToolStripMenuItem_Click(object sender, EventArgs e) {
            smallBlockOverlaysToolStripMenuItem.Checked = !smallBlockOverlaysToolStripMenuItem.Checked;
            GFX.RepatchBlocks(smallBlockOverlaysToolStripMenuItem.Checked);
            Properties.Settings.Default.SmallBlockOverlays = smallBlockOverlaysToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
            Level.ReRenderAll();
            Invalidate(true);
        }

        private void uncheckModeButtons()
        {
            foreach (ToolStripButton b in EditionModeButtons)
                b.Checked = false;
        }
        
        private void editObjectsButton_Click(object sender, EventArgs e) {
            levelEditorControl1.SetEditionMode(oem);
            uncheckModeButtons();
            editObjectsButton.Checked = true;
        }

        private void editEntrancesButton_Click(object sender, EventArgs e) {
            levelEditorControl1.SetEditionMode(eem);
            uncheckModeButtons();
            editEntrancesButton.Checked = true;
        }

        private void editPathsButton_Click(object sender, EventArgs e) {
            levelEditorControl1.SetEditionMode(pem);
            uncheckModeButtons();
            editPathsButton.Checked = true;
        }

        private void editViewsButton_Click(object sender, EventArgs e)
        {
            levelEditorControl1.SetEditionMode(vem);
            uncheckModeButtons();
            editViewsButton.Checked = true;

        }

        private void editZonesButton_Click(object sender, EventArgs e)
        {
            levelEditorControl1.SetEditionMode(zem);
            uncheckModeButtons();
            editZonesButton.Checked = true;
        }

        private void deleteAllObjectsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Properties.Settings.Default.Language != 1) {
                if (MessageBox.Show("Are you sure you want to delete every object in the level?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            } else {
                if (MessageBox.Show("Estas seguro que quieres borrar todos los objetos en el nivel?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            }

            Level.Objects.Clear();
            if(levelEditorControl1.mode != null)
                levelEditorControl1.mode.Refresh();
            levelEditorControl1.Invalidate(true);
            Dirty = true;
        }

        private void deleteAllSpritesToolStripMenuItem_Click(object sender, EventArgs e) {
            if (Properties.Settings.Default.Language != 1) {
                if (MessageBox.Show("Are you sure you want to delete every sprite in the level?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            } else {
                if (MessageBox.Show("Estas seguro que quieres borrar todos los sprites en el nivel?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            }

            Level.Sprites.Clear();
            if(levelEditorControl1.mode != null)
                levelEditorControl1.mode.Refresh();
            levelEditorControl1.Invalidate(true);
            Dirty = true;
        }

        private void spriteFinder_Click(object sender, EventArgs e)
        {
            tools.Show();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            levelEditorControl1.cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            levelEditorControl1.copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            levelEditorControl1.paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            levelEditorControl1.delete();
        }

        private void toolStripDropDownButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripMenuItem it in zoomMenu.DropDown.Items)
                it.Checked = false;
            (e.ClickedItem as ToolStripMenuItem).Checked = true;

            String s = e.ClickedItem.Text;

            int ind = s.IndexOf(" %");
            s = s.Remove(ind);

            float z = Int32.Parse(s);
            levelEditorControl1.SetZoom(z / 100);
        }

        private void editTileset_Click(object sender, EventArgs e)
        {
            new TilesetEditor(GFX.Tilesets[1], GFX).Show();
        }

    }
}