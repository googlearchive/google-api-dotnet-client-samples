/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Google.Apis.Tasks.v1.Data;

namespace TasksExample.WinForms.NoteMgr
{
    /// <summary>
    /// A single graphical note.
    /// </summary>
    public partial class NoteItem : UserControl
    {
        /// <summary>
        /// The text this note contains.
        /// </summary>
        [Category("Note")]
        public string NoteText
        {
            get { return text.Text; }
            set { text.Text = value; }
        }

        /// <summary>
        /// Whether this note is finished or not.
        /// </summary>
        [Category("Note")]
        public bool NoteFinished
        {
            get { return checkbox.Checked; }
            set { checkbox.Checked = value; }
        }

        /// <summary>
        /// The related remote task.
        /// </summary>
        public Task RelatedTask { get; set; }

        /// <summary>
        /// Called whenever the user wants to create a new note.
        /// </summary>
        public event Action NewNoteRequest;

        /// <summary>
        /// Called whenever the user wants to delete a note.
        /// </summary>
        public event Action<NoteItem> DeleteNoteRequest;

        /// <summary>
        /// Called whenever this note is changed by the user.
        /// </summary>
        public event Action<NoteItem> OnNoteChanged;

        public NoteItem()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
        }

        /// <summary>
        /// Creates a new note item and associates the action events with the specified NoteForm.
        /// </summary>
        public NoteItem(NoteForm form, Task relatedTask)
            : this()
        {
            NewNoteRequest += form.AddNote;
            DeleteNoteRequest += form.DeleteNote;
            RelatedTask = relatedTask;

            if (relatedTask != null)
            {
                // Use data from the related task.
                NoteText = relatedTask.Title;
                NoteFinished = relatedTask.Status == "completed";
            }
        }

        /// <summary>
        /// Focuses this note.
        /// </summary>
        public void FocusNote()
        {
            text.Focus();
        }

        /// <summary>
        /// Synchronizes changes between this visual note and the related task.
        /// </summary>
        /// <returns>True if there have been any changes.</returns>
        public bool ClientSync()
        {
            if (RelatedTask == null)
            {
                RelatedTask = new Task();
                RelatedTask.Title = NoteText;
                RelatedTask.Status = NoteFinished ? "completed" : "needsAction";
                return true; // Nothing to sync here.
            }

            bool changes = false;

            // Detect changes.
            if (NoteText != RelatedTask.Title)
            {
                RelatedTask.Title = NoteText;
                changes = true;
            }
            if (NoteFinished != RelatedTask.Completed.HasValue)
            {
                RelatedTask.Status = NoteFinished ? "completed" : "needsAction";
                changes = true;
            }
            return changes;
        }

        private void NoteItem_Paint(object sender, PaintEventArgs e)
        {
            // Draw a small, red line on the bottom.
            int y = ClientSize.Height - 1;
            e.Graphics.DrawLine(Pens.Pink, 0, y, ClientSize.Width, y);
        }

        private void text_KeyDown(object sender, KeyEventArgs e)
        {
            if (text.Text.Length == 0 && e.KeyCode == Keys.Back && DeleteNoteRequest != null)
            {
                // Delete request.
                DeleteNoteRequest(this);
            }
            else if (text.Text.Length > 0 && e.KeyCode == Keys.Enter && NewNoteRequest != null)
            {
                // New note request.
                NewNoteRequest();
            }
        }

        private void text_TextChanged(object sender, EventArgs e)
        {
            if (text.TextLength > 0 && OnNoteChanged != null)
            {
                OnNoteChanged(this);
            }
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            text.Font = new Font(text.Font, checkbox.Checked ? FontStyle.Strikeout : FontStyle.Regular);
            text.ForeColor = checkbox.Checked ? Color.DarkGray : Color.Black;
        }
    }
}
