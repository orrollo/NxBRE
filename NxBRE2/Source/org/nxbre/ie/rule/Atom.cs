namespace org.nxbre.ie.rule {
	using System;
	using System.Text;
	using System.Collections;
	
	using org.nxbre.ie.core;
	using org.nxbre.ie.predicates;
	using org.nxbre.util;
	
	/// <summary>
	/// An Atom represents a typed association of predicates. It is immutable (because predicates are immutable so Members will not vary).
	/// </summary>
	/// <remarks>The Atom supports the method for data pattern matching that are the core of
	/// the forward-chaining (data driven) inference engine.</remarks>
	/// <author>David Dossot</author>
	public class Atom:ICloneable {
		private readonly bool negative;
		private readonly string type;
		private readonly IPredicate[] predicates;
		private readonly string signature;
		private readonly string[] slotNames;
		
		private readonly int hashCode;
		private readonly long longHashCode;
		private readonly string stringLongHashCode;
		private readonly bool isFact;
		private readonly bool hasSlot;
		private readonly bool hasFormula;
		private readonly bool hasFunction;
		private readonly bool hasIndividual;
		
		/// <summary>
		/// Negative fact.
		/// </summary>
		public bool Negative {
			get {
				return negative;
			}
		}
		
		/// <summary>
		/// The type of Atom.
		/// </summary>
		public string Type {
			get {
				return type;
			}
		}
		
		/// <summary>
		/// The array of predicates associated in the Atom.
		/// </summary>
		public IPredicate[] Members {
			get {
				return predicates;
			}
		}
		
		/// <summary>
		/// The array of slot names in the Atom. Non-named members have a String.Empty slot name.
		/// </summary>
		public string[] SlotNames {
			get {
				return slotNames;
			}
		}
		
		/// <summary>
		/// Returns True if there is at least one Slot in the members.
		/// </summary>
		public bool HasSlot {
			get {
				return hasSlot;
			}
		}
		
		/// <summary>
		/// Checks if the current Atom is a Fact by analyzing the predicates.
		/// </summary>
		/// <description>A Fact is an association of predicates of type Individual.</description>
		/// <returns>True if the Atom is a Fact.</returns>
		public bool IsFact {
			get {
				return isFact;
			}
		}
		
		/// <summary>
		/// Returns True if there is at least one Function predicate in the members.
		/// </summary>
		public bool HasFunction {
			get {
				return hasFunction;
			}
		}
		
		/// <summary>
		/// Returns True if there is at least one Formula predicate in the members.
		/// </summary>
		public bool HasFormula {
			get {
				return hasFormula;
			}
		}
		
		/// <summary>
		/// Returns True if there is in the members at least one Individual predicate.
		/// </summary>
		public bool HasIndividual {
			get {
				return hasIndividual;
			}
		}
		
		/// <summary>
		/// Returns True if there is in the members at least one Variable predicate.
		/// </summary>
		public bool HasVariable {
			get {
				return !isFact;
			}
		}
		
		/// <summary>
		/// A String signature used for internal purposes
		/// </summary>
		internal string Signature {
			get {
				return signature;
			}
		}
		
		/// <summary>
		/// Pre-casted string rendering of the long hashcode
		/// </summary>
		internal string StringLongHashCode {
			get {
				return stringLongHashCode;
			}
		}
		
		/// <summary>
		/// Instantiates a new Positive (non-NAF) non-function Relation Atom.
		/// </summary>
		/// <param name="type">Type of the Atom.</param>
		/// <param name="members">Array of predicates associated in the Atom.</param>
		public Atom(string type, params IPredicate[] members):this(false, type, members) {}
		
		/// <summary>
		/// Instantiates a new Atom.
		/// </summary>
		/// <param name="negative">Negative Atom.</param>
		/// <param name="type">The relation type of the Atom.</param>
		/// <param name="members">Array of predicates associated in the Atom.</param>
		/// <remarks>This is the principal constructor for Atom and descendant objects.</remarks>
		public Atom(bool negative, string type, params IPredicate[] members) {
			this.negative = negative;
			this.type = type;
			
			// load the predicates, extracting the slot names if any
			predicates = new IPredicate[members.Length];
			slotNames = new string[members.Length];
			hasSlot = false;
			
			for(int i=0; i<members.Length; i++) {
				if (members[i] is Slot) {
					hasSlot = true;
					Slot slot = (Slot)members[i];
					predicates[i] = slot.Predicate;
					slotNames[i] = slot.Name;
				}
				else {
					predicates[i] = members[i];
					slotNames[i] = String.Empty;
				}
			}
			
			// initialize long hashcode & other characteristics
			longHashCode = (long)type.GetHashCode() << 32 ^ (long)type.GetHashCode();
			isFact = true;
			hasFunction = false;
			hasFormula = false;
			hasIndividual = false;
			
			foreach(IPredicate member in predicates) {
				longHashCode <<= 1;
				longHashCode ^= member.GetLongHashCode();
				if (member is Variable) isFact = false;
				if (member is Function)	hasFunction = true;
				if (member is Formula) hasFormula = true;
				if (member is Individual) hasIndividual = true;
			}
			
			// initialize hashcode
			hashCode = ((int)(longHashCode & 0xFFFFFFFF) ^ (int)((longHashCode >> 32) & 0xFFFFFFFF));
			
			// initialize string hashcode
			stringLongHashCode = longHashCode.ToString();
			
			// initialize signature
			signature = type + predicates.Length;
		}
		
		/// <summary>
		/// Protected constructor used for cloning purpose.
		/// </summary>
		/// <param name="source">The atom to use as a template.</param>
		/// <param name="members">The members to use instead of the ones in the source, or null if the ones of the source must be used.</param>
		protected Atom(Atom source, IPredicate[] members):this(source.negative, source.type, members)	{
			this.slotNames = source.slotNames;
		}
		
		/// <summary>
		/// Protected constructor used for cloning purpose.
		/// </summary>
		/// <param name="source"></param>
		protected Atom(Atom source):this(source, (IPredicate[])source.predicates.Clone()) {}

		/// <summary>
		/// Returns a cloned Atom, of same type and containing a clone of the array of predicates.
		/// </summary>
		/// <returns>A new Atom, based on the existing one.</returns>
		/// <remarks>The predicates are not cloned.</remarks>
		public virtual object Clone() {
			return new Atom(this);
		}
		
		/// <summary>
		/// Performs a clone of the current Atom but substitute members with the provided ones.
		/// </summary>
		/// <param name="members">New members to use.</param>
		/// <returns>A clone with new members.</returns>
		public virtual Atom CloneWithNewMembers(params IPredicate[] members) {
			return new Atom(this, members);
		}
		
		/// <summary>
		/// Checks if the current Atom basic matches with another one, i.e. if they are of same type,
		/// and contain the same number of predicates.
		/// </summary>
		/// <param name="atom">The other atom to determine the basic matching.</param>
		/// <returns>True if the two atoms basic match.</returns>
		public bool BasicMatches(Atom atom) {
			return ((atom.Type == Type) && (atom.predicates.Length == predicates.Length));
		}

		/// <summary>
		/// Checks if the current Atom matches with another one, i.e. if they are of same type,
		/// contain the same number of predicates, and if their Individual predicates are equal.
		/// </summary>
		/// <description>
		/// This functions takes care of casting as it always tries to cast to the strongest type
		/// of two compared individuals. Since predicates can come from weakly-typed rule files
		/// (Strings) and other predicates can be generated by the user, this function tries to
		/// convert from String to the type of the other predicate (as String is considered not
		/// strongly typed).
		/// </description>
		/// <param name="atom">The other atom to determine the matching.</param>
		/// <returns>True if the two atoms match.</returns>
		public bool Matches(Atom atom) {
			if (!BasicMatches(atom)) return false;
			
			for(int i=0; i<predicates.Length; i++) {
				if ((predicates[i] is Individual) &&
				    (atom.predicates[i] is Function) &&
						(!((Function)atom.predicates[i]).Evaluate((Individual)predicates[i])))
					return false;
				
				else if ((predicates[i] is Function) &&
				         (atom.predicates[i] is Individual) &&
								 (!((Function)predicates[i]).Evaluate((Individual)atom.predicates[i])))
					return false;
			
				else if ((predicates[i] is Function) &&
				         (atom.predicates[i] is Function) &&
								 (!(predicates[i].Equals(atom.predicates[i]))))
					return false;
			
				else if ((predicates[i] is Individual) && (atom.predicates[i] is Individual)) {
					// we have two individuals
					if ((predicates[i].Value.GetType() == atom.predicates[i].Value.GetType())
					    && (!predicates[i].Equals(atom.predicates[i]))) {
						// the two individuals are of same types: direct compare
						return false;
					}
					else {
						// the two individuals are of different types
						ObjectPair pair = new ObjectPair(predicates[i].Value, atom.predicates[i].Value);
						Reflection.CastToStrongType(pair);
						if (!pair.First.Equals(pair.Second)) return false;
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// Check if the current intersects with another one, which means that:
		///  - they Match() together,
		///  - their predicate types are similar,
		///  - if there are variables, at least one should be equal to the corresponding one.
		/// </summary>
		/// <param name="atom">The other atom to determine the intersection.</param>
		/// <returns>True if the two atoms intersect.</returns>
		/// <remarks>IsIntersecting calls Matches first.</remarks>
		public bool IsIntersecting(Atom atom) {
			if (!Matches(atom)) return false;

			for(int i=0; i<predicates.Length; i++)
				if (predicates[i].GetType() != atom.predicates[i].GetType())
					return false;
			
			if (!HasVariable) return true;
			
			int nonMatchingVariables = 0;
			int variableCount = 0;
			
			for(int i=0; i<predicates.Length; i++) {
				variableCount++;
				if (predicates[i] is Variable) {
					if (!(predicates[i].Equals(atom.predicates[i])))	nonMatchingVariables++;
				}
			}
	
			if (variableCount < predicates.Length) return true;
			else return (nonMatchingVariables < variableCount);
		}		
		
		/// <summary>
		/// Returns the String representation of the Atom for display purpose only.
		/// </summary>
		/// <returns>The String representation of the Atom.</returns>
		public override string ToString() {
			StringBuilder result = new StringBuilder(negative?"!":"");
			
			result.Append(type).Append("{");
			
			bool first = true;
			
			for(int i=0; i<predicates.Length; i++) {
				IPredicate member = predicates[i];
				
				if (!first) result.Append(",");
				
				if (slotNames[i] != String.Empty) result.Append(slotNames[i]).Append("=");
				
				result.Append(member.ToString());
				
				if (first) first = false;
			}
			
			result.Append("}");
			
			return result.ToString();
		}
		
		/// <summary>
		/// Checks if the current Atom is equal to another one, based on their hashcode.
		/// </summary>
		/// <param name="o">The other Atom to test the equality.</param>
		/// <returns>True if the two atoms are equal.</returns>
		public override bool Equals(object o) {
			if (o.GetType() != this.GetType()) throw new ArgumentException("Compared object must be a " +
			                                                               this.GetType() +
			                                                               " but was " +
			                                                               o.GetType());
			
			return (((Atom)o).GetLongHashCode() == this.GetLongHashCode());
		}

		/// <summary>
		/// Calculates the hashcode of the current Atom.
		/// </summary>
		/// <remarks>
		/// The hashcode is a reduction of the long hashcode, obtained by shifting and XORing the
		/// two Int32 composing the long hashcode. It is therefore less accurate.
		/// </remarks>
		/// <returns>The hashcode of the current Atom.</returns>
		public override int GetHashCode() {
			return hashCode;
		}
		
		/// <summary>
		/// Calculates the long hashcode of the current Atom by combining the hashcodes of its predicates.
		/// The predicates are not permutable, i.e. their position is significant.
		/// </summary>
		/// <returns>The long hashcode of the current Atom.</returns>
		public long GetLongHashCode() {
			return longHashCode;
		}

		/// <summary>
		/// A helper method for easily reaching a member predicate value from its index.
		/// </summary>
		/// <param name="predicateIndex">The index of the predicate in the array of Members.</param>
		/// <returns>The actual value of the predicate, or throws an exception if the index is out of range.</returns>
		public object GetPredicateValue(int predicateIndex) {
			return predicates[predicateIndex].Value;
		}
		
		/// <summary>
		/// A helper method for easily reaching a member predicate value from its slot name.
		/// </summary>
		/// <param name="slotName">The name of the slot in which the predicate is stored</param>
		/// <returns>The actual value of the predicate, or throws an exception if no slot matches the name.</returns>
		public object GetPredicateValue(string slotName) {
			IPredicate predicate = GetPredicate(slotName);
			if (predicate == null) throw new ArgumentException("There is no slot named: " + slotName);
			else return predicate.Value;
		}
		
		/// <summary>
		/// A helper method for easily reaching a member predicate from its slot name.
		/// </summary>
		/// <param name="slotName">The name of the slot in which the predicate is stored</param>
		/// <returns>The predicate or null if no slot matches the name.</returns>
		public IPredicate GetPredicate(string slotName) {
			if ((slotName == null) || (slotName == String.Empty)) throw new ArgumentException("The name of a slot can not be null or empty");
			
			int slotIndex = Array.IndexOf(slotNames, slotName);
			
			if (slotIndex < 0) return null;
			else return predicates[slotIndex];
		}
		
		/// <summary>
		/// A helper accessor for easily getting the member predicate values.
		/// </summary>
		/// <returns>An array of objects containing the member predicate values.</returns>
		/// <remarks>
		/// This method is inefficient performance-wise and should be used wisely.
		/// </remarks>
		public object[] PredicateValues {
			get {
				ArrayList values = new ArrayList();
				foreach(IPredicate member in predicates)values.Add(member.Value);
				return values.ToArray();
			}
		}
		
		// ----------------- Static methods ----------------
		
		/// <summary>
		/// Resolves all Function predicates by replacing them by their String representations.
		/// </summary>
		/// <param name="atom">The Atom to resolve.</param>
		/// <returns>A new Atom where all Function predicates have been resolved. If no
		/// Function predicate exists, it returns a clone of the current Atom.</returns>
		public static Atom ResolveFunctions(Atom atom) {
			if (atom.HasFunction) {
				IPredicate[] predicates = new IPredicate[atom.predicates.Length];
				
				for(int i=0; i<atom.predicates.Length; i++)
					if (atom.predicates[i] is Function) predicates[i] = new Individual(atom.predicates[i].ToString());
					else predicates[i] = atom.predicates[i];
				return new Atom(atom.Negative, atom.Type, predicates);
			}
			else
				return (Atom)atom.Clone();
		}
		
		/// <summary>
		/// Translates variable names of a target atom with names from a template atom matching the position of a
		/// source atom.
		/// </summary>
		/// <remarks>Template and source atoms must match together else unpredictible result may occur.</remarks>
		/// <param name="template"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Atom TranslateVariables(Atom template, Atom source, Atom target) {
			IPredicate[] resultMembers = new IPredicate[target.Members.Length];
			
			for(int i=0; i<target.Members.Length; i++) {
				IPredicate targetMember = target.Members[i];
				
				if (targetMember is Variable) {
					int indexOfSourceMember = Array.IndexOf(source.Members, targetMember);
					if (indexOfSourceMember >= 0) resultMembers[i] = template.Members[indexOfSourceMember];
					else resultMembers[i] = targetMember;
				}
				else
					resultMembers[i] = targetMember;
			}
			
			return new Atom(template.Negative, target.Type, resultMembers);
		}
		
	}

}
