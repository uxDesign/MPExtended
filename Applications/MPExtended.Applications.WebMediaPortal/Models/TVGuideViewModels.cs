﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPExtended.Libraries.General;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class TVGuideViewModel
    {
        public DateTime GuideStart { get; private set; }
        public DateTime GuideEnd { get; private set; }

        public int GroupId { get; private set; }
        public string GroupName { get; private set; }
        public IEnumerable<TVGuideChannelViewModel> Channels { get; private set; }

        public TVGuideViewModel(WebChannelGroup channelGroup, DateTime guideStart, DateTime guideEnd)
        {
            GuideStart = guideStart;
            GuideEnd = guideEnd;

            GroupId = channelGroup.Id;
            GroupName = channelGroup.GroupName;

            DateTime loadGuideEnd =guideEnd.Subtract(TimeSpan.FromSeconds(1)); // do not load programs that start at the end of the guid
            Channels = MPEServices.TAS.GetChannelsBasic(channelGroup.Id).Select(x => new TVGuideChannelViewModel(x, guideStart, loadGuideEnd));
        }
    }

    public class TVGuideChannelViewModel
    {
        public int Id { get; private set; }
        public string DisplayName { get; private set; }
        public IEnumerable<TVGuideProgramViewModel> Programs { get; private set; }

        private IEnumerable<WebProgramBasic> programList;

        public TVGuideChannelViewModel(WebChannelBasic channel, DateTime guideStart, DateTime guideEnd)
        {
            Id = channel.Id;
            DisplayName = channel.DisplayName;

            programList = MPEServices.TAS.GetProgramsBasicForChannel(Id, guideStart, guideEnd);
            Programs = programList.Select(x => new TVGuideProgramViewModel(x, guideStart, guideEnd));
        }
    }

    public class TVGuideProgramViewModel
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        private DateTime guideStart;
        private DateTime guideEnd;

        public TVGuideProgramViewModel(WebProgramBasic program, DateTime guideStart, DateTime guideEnd)
        {
            Id = program.Id;
            Title = program.Title;
            StartTime = program.StartTime;
            EndTime = program.EndTime;

            this.guideStart = guideStart;
            this.guideEnd = guideEnd;
        }

        public string GetPercentageWidth()
        {
            // get duration of program
            DateTime calcStartTime = StartTime < guideStart ? guideStart : StartTime;
            DateTime calcEndTime = EndTime > guideEnd ? guideEnd : EndTime;
            double programDuration = (calcEndTime - calcStartTime).TotalMinutes;
            double guideDuration = (guideEnd - guideStart).TotalMinutes;

            // calculate percentage
            var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            return string.Format(invariantCulture, "{0:0.00}%", programDuration / guideDuration * 100);
        }
    }
}