namespace ProjectNZM
{
    public class State
    {
      public string Name {get; set;}
      public bool IsAccepted{get; set;}
      private Dictionary<char,State> transitions;
      public State(string name,bool isAccepted =false)
        {
            Name=name;
            IsAccepted=isAccepted;
            transitions = new Dictionary<char, State>();
        }  
        public void Addtransition(char symbol,State target)
        {
            if(!transitions.ContainsKey(symbol))
            {
                transitions.Add(symbol,target);
            }
            else
            {
                throw new Exception($"A transition for '{symbol}' exists in the state {Name}");
            }
        }
        public State GetNextState(char symbol)
        {
            if(transitions.ContainsKey(symbol))
            {
                return transitions[symbol];
            
            }
            else
            {
                return null;
            }
        }
        public Dictionary<char,State> GetTransitions()
        {
            return transitions;
        }
    }
    public class DFA
    {
        List<State> states;
        HashSet<char> Symbol; //each iterative symbol won't be saved
        State startstate;
        public DFA()
        {
            states = new List<State>();
            Symbol = new HashSet<char>();
        }
        // methods for ALi
    }
    public class DFA_builder
    {
        DFA dfa;
        Dictionary<string,State>state;
        public DFA_builder()
        {
            dfa=new DFA();
            state =new Dictionary<string, State>();
        }
        //methods for Sajad
    }
}