using System;

namespace TripleS {

    public class ParamExecption : Exception {
        public ParamExecption(string message)
            : base(message)
        {
        }
    }

    public class GameStateExecption : Exception {
        public GameStateExecption(string message)
            : base(message)
        {
        }
    }

    public class ScriptExecption : Exception {
        public ScriptExecption(string message)
            : base(message)
        {
        }
    }
}
