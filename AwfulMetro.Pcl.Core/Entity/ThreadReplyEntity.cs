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
namespace AwfulMetro.Core.Entity
{
    public class ThreadReplyEntity
    {
        public ThreadReplyEntity(long threadId, string post, string formKey, string formCookie)
        {
            ThreadId = threadId;
            Post = post;
            FormKey = formKey;
            FormCookie = formCookie;
        }

        public long ThreadId { get; private set; }

        public string Post { get; private set; }

        public string FormKey { get; private set; }

        public string FormCookie { get; private set; }
    }
}