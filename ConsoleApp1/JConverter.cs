using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class JConverter
    {
        public static dynamic JsonConvertDynamic(string line)
        {
            try
            {
                return JsonConvert.DeserializeObject(line);
            }
            catch (JsonReaderException jrex)
            {
                throw jrex;
            }
            catch (JsonSerializationException jrex)
            {
                throw jrex;
            }
            catch (JsonWriterException jrex)
            {
                throw jrex;
            }
            catch (Exception jrex)
            {
                throw jrex;
            }
        }
    }
}
