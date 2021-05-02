namespace EmailCoreInfra
{
    using System.Collections.Generic;

    public class EmailTempalteData
    {
        public List<string> ToEmailAdresses { get; set; }
        public string ReplacementTemplateDataAsJson { get; set; }
    }
}
