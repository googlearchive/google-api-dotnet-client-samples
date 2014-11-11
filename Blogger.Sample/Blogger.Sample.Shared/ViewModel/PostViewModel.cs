/*
Copyright 2014 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using GalaSoft.MvvmLight;

namespace Blogger.Sample.ViewModel
{
    /// <summary>A single post view model.</summary>
    public class PostViewModel : ViewModelBase
    {
        private string content;
        /// <summary>Gets or sets the post content.</summary>
        public string Content
        {
            get { return content; }
            set
            {
                Set(() => Content, ref content, value);
            }
        }

        private string title;
        /// <summary>Gets or sets the post title.</summary>
        public string Title
        {
            get { return title; }
            set
            {
                Set(() => Title, ref title, value);
            }
        }
    }
}
