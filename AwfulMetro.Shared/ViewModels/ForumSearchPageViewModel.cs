﻿#region copyright
/*
Awful Metro - A Modern UI Something Awful Forums Viewer
Copyright (C) 2014  Tim Miller

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AwfulMetro.Common;
using AwfulMetro.Pcl.Core.Entity;
using AwfulMetro.Pcl.Core.Manager;

namespace AwfulMetro.ViewModels
{
    public class ForumSearchPageViewModel : NotifierBase
    {
        private bool _isLoading;
        private List<ForumUserSearchEntity> _forumUserList;

        public List<ForumUserSearchEntity> ForumUserList
        {
            get { return _forumUserList; }
            set
            {
                SetProperty(ref _forumUserList, value);
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged();
            }
        }

        public async void GetUsernameList(string username)
        {
            IsLoading = true;
            var searchManager = new SearchManager();
            ForumUserList = await searchManager.GetUsernames(username);
            IsLoading = false;
        }
    }
}
