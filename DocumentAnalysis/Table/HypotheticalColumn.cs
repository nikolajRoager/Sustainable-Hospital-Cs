namespace DocumentAnalysis
{

    /// <summary>
    /// A hypothetical column in the analyzed, which may still be somewhat ambiguos as to what it is
    /// </summary>
    /*package private*/class HypotheticalColumn

    {
        /// <summary>
        /// How many different things can this column be
        /// </summary>
        public int ambiguoity { get{return (couldBeNumber?1:0)+(couldBeProduct?1:0)+(couldBeSingleMass?1:0)+(couldBeAmount?1:0)+(couldBeTotalMass?1:0);} }
        public bool couldBeProduct { get; set; } = false;
        public bool couldBeNumber { get; set; } = false;
        public bool couldBeSingleMass { get; set; } = false;
        public bool couldBeTotalMass { get; set; } = false;
        public bool couldBeAmount {get; set;} = false;
        /// <summary>
        /// A column may be spread out over multiple columns for some stupid reason
        /// </summary>
        public bool isSpreadOut { get; set; } = false;
        public int header_y { get; set; } 
        public int column_x { get; set;} 
        public int height { get; set; } = 0;
    }

}