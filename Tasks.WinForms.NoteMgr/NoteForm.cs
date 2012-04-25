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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Google.Apis.Tasks.v1.Data;

namespace TasksExample.WinForms.NoteMgr
{
    /// <summary>
    /// Visual representation of a tasklist.
    /// </summary>
    public partial class NoteForm : Form
    {
        private readonly List<NoteItem> deletedNotes = new List<NoteItem>();
        private readonly TaskList taskList;
        private readonly object sync = new object();

        public NoteForm()
        {
            InitializeComponent();
            AddNote();
        }

        public NoteForm(TaskList taskList)
        {
            InitializeComponent();
            this.taskList = taskList;
            
            // Set the title.
            Text = taskList.Title;

            // Load all notes:
            Tasks tasks = Program.Service.Tasks.List(taskList.Id).Fetch();
            if (tasks.Items != null)
            {
                foreach (Task task in tasks.Items)
                {
                    AddNote(task);
                }
            }
            else
            {
                AddNote();
            }
        }

        /// <summary>
        /// Adds a new empty note to the note list.
        /// </summary>
        public void AddNote()
        {
            AddNote(null).FocusNote();
        }

        /// <summary>
        /// Loads the specified note and adds it to the form.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public NoteItem AddNote(Task task)
        {
            var newNote = new NoteItem(this, task);

            // Insert the new control as the first element, as it will be displayed on the bottom.
            var all = (from Control c in Controls select c).ToArray();
            SuspendLayout();
            Controls.Clear();
            Controls.Add(newNote);
            Controls.AddRange(all);
            UpdateHeight();
            ResumeLayout();
            
            return newNote;
        }
        
        /// <summary>
        /// Deletes a note from the note list.
        /// </summary>
        public void DeleteNote(NoteItem note)
        {
            if (Controls.Count <= 1)
            {
                return; // Don't remove the last note.
            }
            
            Controls.Remove(note);
            deletedNotes.Add(note);
            ((NoteItem)Controls[0]).FocusNote();
            UpdateHeight();
        }

        /// <summary>
        /// Synchronizes this client with the remote server.
        /// </summary>
        public void ClientSync()
        {
            // TODO(mlinder): Implement batching here.
            lock (sync)
            {
                var requests = new List<Action>();

                // Add changes/inserts.
                NoteItem previous = null;
                foreach (NoteItem currentNote in (from Control c in Controls where c is NoteItem select c).Reverse())
                {
                    NoteItem note = currentNote;
                    if (note.ClientSync())
                    {
                        bool isNew = String.IsNullOrEmpty(note.RelatedTask.Id);
                        requests.AddRange(GetSyncNoteRequest(note, previous, isNew));
                    }
                    previous = note; 
                }

                // Add deletes.
                foreach (NoteItem note in deletedNotes)
                {
                    NoteItem noteb = note;
                    if(note.RelatedTask != null && !String.IsNullOrEmpty(note.RelatedTask.Id))
                        requests.Add(() => Program.Service.Tasks.Delete(taskList.Id, noteb.RelatedTask.Id).Fetch());
                }
                deletedNotes.Clear();

                // Execute all requests.
                requests.ForEach(action => action());
            }
        }

        private IEnumerable<Action> GetSyncNoteRequest(NoteItem note, NoteItem previous, bool isNew)
        {
            var tasks = Program.Service.Tasks;

            if (isNew)
            {
                NoteItem previousSaved = previous;
                yield return () => note.RelatedTask = tasks.Insert(note.RelatedTask, taskList.Id).Fetch();
                yield return () =>
                                 {
                                     var req = tasks.Move(taskList.Id, note.RelatedTask.Id);
                                     if (previousSaved != null)
                                     {
                                         req.Previous = previousSaved.RelatedTask.Id;
                                     }
                                     note.RelatedTask = req.Fetch();
                                 };
            }
            else
            {
                yield return
                    () =>
                        {
                            var req = tasks.Update(note.RelatedTask, taskList.Id, note.RelatedTask.Id);
                            note.RelatedTask = req.Fetch();
                        };
            }
        }

        private void UpdateHeight()
        {
            // Change the height of the list to contain all items.
            int totalY = 0;
            foreach (Control c in Controls)
            {
                totalY += c.Height;
            }
            ClientSize = new Size(ClientSize.Width, totalY);
        }

        private void NoteForm_Shown(object sender, System.EventArgs e)
        {
            // Add a sync timer.
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000 * 60 * 5; // 5 min
            timer.Tick += (timerSender, timerArgs) => ClientSync();
            timer.Start();

            // Focus the first note.
            if (Controls.Count > 0)
            {
                ((NoteItem)Controls[0]).FocusNote();
            }
        }

        private void NoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClientSync();
            Application.DoEvents();
            lock (sync) {}
        }

        private void NoteForm_Deactivate(object sender, EventArgs e)
        {
            if (Disposing)
            {
                return;
            }

            // Sync asynchronously.
            ThreadPool.QueueUserWorkItem((obj) => ClientSync());
        }

        private void NoteForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms.Count == 0)
            {
                // If no more forms are open, exit the WinForms worker thread.
                Application.Exit();
            }
        }
    }
}
