using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using SwingCommon;

namespace SwingDataViewer.Models
{
	public static class ColorTable
	{
		public static Dictionary<ClubType, Color> DefaultPalette = new Dictionary<ClubType, Color> {
			{ClubType.W1, Color.FromArgb(128,255,0,50)},
			{ClubType.W3, Color.FromArgb(128,255,0,105)},
			{ClubType.W5, Color.FromArgb(128,255,0,155)},
			{ClubType.W7, Color.FromArgb(128,255,0,205)},
			{ClubType.W9, Color.FromArgb(128,255,0,255)},
			{ClubType.U2, Color.FromArgb(128,255,180,0)},
			{ClubType.U3, Color.FromArgb(128,255,160,0)},
			{ClubType.U4, Color.FromArgb(128,255,140,0)},
			{ClubType.U5, Color.FromArgb(128,255,120,0)},
			{ClubType.U6, Color.FromArgb(128,255,100,0)},
			{ClubType.I3, Color.FromArgb(128,0,255,94)},
			{ClubType.I4, Color.FromArgb(128,0,228,48)},
			{ClubType.I5, Color.FromArgb(128,0,208,0)},
			{ClubType.I6, Color.FromArgb(128,0,188,0)},
			{ClubType.I7, Color.FromArgb(128,0,168,0)},
			{ClubType.I8, Color.FromArgb(128,0,148,0)},
			{ClubType.I9, Color.FromArgb(128,0,128,0)},
			{ClubType.PW, Color.FromArgb(128,0,0,255)},
			{ClubType.AW, Color.FromArgb(128,0,0,228)},
			{ClubType.SW, Color.FromArgb(128,0,0,198)},
			{ClubType.LW, Color.FromArgb(128,0,0,168)},
			{ClubType.PT, Color.FromArgb(128,0,0,0)},
		};

		public static string GetClubColor(ClubType club)
		{
			if (!DefaultPalette.TryGetValue(club, out var color)) return "rgb(255,255,255)";
			return $"rgb({color.R},{color.G},{color.B})";
		}
	}
}
