using System;

namespace MvcApplication1
{
    public static class Parser
    {

        /// <summary>
        /// Return the output line after [output].
        /// 
        /// For example, in line = [INFO_TYPE]=ITEM||[ITEM_NAME]=CLOTHING||[ITEM_PRICE]=49.22||....
        ///     Calling this program:
        /// 
        ///     
        ///     Parse_Line_Information(line, "ITEM_PRICE", parse_token = "||") returns "49.22"
        ///     
        /// </summary>
        public static string Parse(string input, string output, string parse_token = "||", string default_string = "")
        {
            string[] Split_Layer_1 = input.Split(new string[] { parse_token }, StringSplitOptions.None);

            foreach (string Info_Pair in Split_Layer_1)
            {
                if (Info_Pair.Contains("[" + output + "]"))
                {
                    return Info_Pair.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                }
            }
            //Diagnostics.WriteLine("Potential error with Parse Line info for output: " + output);
            return default_string;
        }

    }

}