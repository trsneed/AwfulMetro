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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwfulMetro.Core.Entity
{
    public class NewPrivateMessageEntity
    {
        public PostIconEntity Icon { get; set; }

        public string Title { get; set; }

        public string Receiver { get; set; }

        public string Body { get; set; }

        public void MapTo(PostIconEntity icon, string title, string receiver, string body)
        {
            this.Icon = icon;
            this.Title = title;
            this.Receiver = receiver;
            this.Body = body;
        }
    }
}
