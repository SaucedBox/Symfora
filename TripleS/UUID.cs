using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleS {

    public class UUID {
        public static List<int> InternalUUIDs { get; private set; }
        private static Random generator;
        private static int genLength;
        private static int lastMaster;

        public int MainUUID { get; private set; }
        public int Master { get; private set; }

        public UUID(int master)
        {
            Master = master;
            MainUUID = GenerateUUID(master);
        }

        private static int GenerateUUID(int master)
        {
            if (generator == null)
                generator = new Random();
            if (InternalUUIDs == null)
                InternalUUIDs = new List<int>();

            if (master > 9 || master < 1)
                throw new Exception("Master in UUID is out of bounds.");

            var canidate = generator.Next(0, 9999);
            canidate += master * 10000;

            if (lastMaster == master && genLength >= 9999)
                throw new Exception("Master in UUID is out of bounds.");

            lastMaster = master;
            genLength++;

            if (InternalUUIDs.Contains(canidate))
                return GenerateUUID(master);
            else
                return canidate;       
        }
    }

    public interface UUIDItem
    {
        public UUID UUID { get; }
    }
}
