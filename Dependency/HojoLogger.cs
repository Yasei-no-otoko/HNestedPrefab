using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace HojoSystem.Utility
{
	public class HojoLogger
	{
		public enum LoggerColor
		{
			EmphasizedCyan = 0x045FB4,
			SuccessfulGreen = 0x088A4B,
			WarningOrange = 0xB45F04,
			SmallInformationGray = 0x484E52
		}

		static private ILogger logger = UnityEngine.Debug.logger;

		[Conditional ("UNITY_EDITOR")]
		public static void Log (object log, LoggerColor color)
		{
			logger.Log (string.Format ("<Color=#{0}>{1}</color>", ((int)color).ToString ("X6"), log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void Log (object log)
		{
			logger.Log (string.Format ("{0}", log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogWithBlackets (object log)
		{
			logger.Log (string.Format ("<b>【{0}】</b>", log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogWithBlackets (object log, LoggerColor color)
		{
			logger.Log (string.Format ("<b><Color=#{0}>【{1}】</color></b>", ((int)color).ToString ("X6"), log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogWarning (object log, LoggerColor color)
		{
			logger.LogWarning ("WARNING", string.Format ("<Color=#{0}>{1}</color>", ((int)color).ToString ("X6"), log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogWarning (object log)
		{
			logger.LogWarning ("WARNING", string.Format ("{0}", log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogError (object log, LoggerColor color)
		{
			logger.LogError ("ERROR", string.Format ("<Color=#{0}>{1}</color>", ((int)color).ToString ("X6"), log));
		}

		[Conditional ("UNITY_EDITOR")]
		public static void LogError (object log)
		{
			logger.LogError ("ERROR", string.Format ("{0}", log));
		}



	}

}