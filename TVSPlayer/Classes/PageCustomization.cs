using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVSPlayer { 
    /// <summary>
    /// Classe used for customizing MainWindow top bar function and design
    /// </summary>
    public class PageCustomization{
       
        /// <summary>
        /// Title that is displayed in the bar on top
        /// </summary>
        public string MainTitle { get; set; }
        
        /// <summary>
        /// Event for the search bar that is part of the bar on top
        /// </summary>
        public System.Windows.Controls.TextChangedEventHandler SearchBarEvent { get; set; }
       
        /// <summary>
        /// Additional buttons for the button on the right of top bar
        /// </summary>
        public UIElement Buttons { get; set; }

    }
}
