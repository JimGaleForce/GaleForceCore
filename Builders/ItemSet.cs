//-----------------------------------------------------------------------
// <copyright file="SimpleSqlBuilder`4.cs" company="Gale-Force">
// Copyright (C) Gale-Force. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace GaleForceCore.Builders
{
    /// <summary>
    /// Class ItemSet.
    /// </summary>
    public class ItemSet
    {
        /// <summary>
        /// Gets or sets the item1.
        /// </summary>
        public object Item1 { get; set; }

        /// <summary>
        /// Gets or sets the item2.
        /// </summary>
        public object Item2 { get; set; }

        /// <summary>
        /// Gets or sets the item3.
        /// </summary>
        public object Item3 { get; set; }

        public object Item4 { get; set; }

        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 1:
                        return this.Item1;
                    case 2:
                        return this.Item2;
                    case 3:
                        return this.Item3;
                    case 4:
                        return this.Item4;
                }

                return null;
            }

            set
            {
                switch (index)
                {
                    case 1:
                        this.Item1 = value;
                        break;
                    case 2:
                        this.Item2 = value;
                        break;
                    case 3:
                        this.Item3 = value;
                        break;
                    case 4:
                        this.Item4 = value;
                        break;
                }
            }
        }
    }
}