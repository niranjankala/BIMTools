using Datacubist.simplebim.Developer.Desktop.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datacubist.Common
{
    public class FrameBroker
    {

        private static FrameBroker _broker;

        private Datacubist.simplebim.Developer.Desktop.Frame.DockFrame _frame;

        private FrameBroker()
        {
        }

        public static FrameBroker Create()
        {

            // Create only once
            if (_broker == null)
            {
                _broker = new FrameBroker();
            }

            return _broker;
        }

        public static Datacubist.simplebim.Developer.Desktop.Frame.DockFrame Frame
        {
            get { return FrameBroker.Create()._frame; }
            set { FrameBroker.Create()._frame = value; }
        }

        public ApplicationWorkspace GetApplicationWorkspace()
        {
            return _frame.Workspace;
        }
    }

}
