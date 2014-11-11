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

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Blogger.Sample.Common;
using Blogger.Sample.Repository;

namespace Blogger.Sample.ViewModel
{
    /// <summary>The blogs view model which contains all blog data.</summary>
    public class BlogsViewModel : ViewModelBase
    {
        /// <summary>Gets the blog view models. Updating this collection should be from the UI thread.</summary>
        public ObservableCollection<BlogViewModel> Blogs { get; private set; }
        private BlogViewModel selectedBlog;

        /// <summary>Gets or sets the selected blog.</summary>
        public BlogViewModel SelectedBlog
        {
            get { return selectedBlog; }
            set
            {
                Set(() => SelectedBlog, ref selectedBlog, value);
                if (selectedBlog != null)
                    selectedBlog.RefreshPosts();
            }
        }

        private readonly IBloggerRepository repository;

        /// <summary>Gets the get blogs command.</summary>
        public RelayCommand GetBlogsCommand { get; private set; }

        public BlogsViewModel(IBloggerRepository repository)
        {
            Blogs = new ObservableCollection<BlogViewModel>();
            this.repository = repository;

            this.GetBlogsCommand = new RelayCommand(
                async () => await GetBlogsAsync());
        }

        /// <summary>Asynchronously gets all the blogs.</summary>
        public async Task GetBlogsAsync()
        {
            var blogs = await repository.GetBlogsAsync();

            // Fill the blogs collection should be from the thread that created the collection.
            await UIUtils.InvokeFromUIThread(() =>
            {
                Blogs.Clear();
                foreach (var b in blogs)
                {
                    Blogs.Add(new BlogViewModel(repository)
                    {
                        Name = b.Name,
                        Id = b.Id
                    });
                }
            });
        }
    }
}
