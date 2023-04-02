using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.HelperClasses
{
	public class RoleSurvey_HM
	{
		public RoleSurvey MainInstance { get; set; }
		public string RoleName { get; set; }
		public List<RoleSurveyOption_HM> Options { get; set; } = new();
		public List<RoleSurveyOption_HM> Triggers { get; set; } = new();		
	}
}
