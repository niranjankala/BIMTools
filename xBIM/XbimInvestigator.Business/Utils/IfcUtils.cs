using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace XbimInvestigator.Business.Utils
{
    public class IfcUtils
    {
        public XTreeNode CreateProjectHierarchy(IfcProject project)
        {
            XTreeNode projectNode = null;
            if (project != null)
            {
                projectNode = CreateObject(project);

                PrintHierarchy(project, projectNode);

            }
           return projectNode;
        }

        XTreeNode CreateObject(IIfcObjectDefinition project)
        {
            XTreeNode projectNode = new XTreeNode(project.Name, project.GlobalId);
            return projectNode;

        }

        private void PrintHierarchy(IIfcObjectDefinition o, XTreeNode parentNode)
        {
            //textBox1.Text += ($"{GetIndent(level)}{o.Name} [{o.GetType().Name}]");
            var spatialElement = o as IIfcSpatialStructureElement;
            if (spatialElement != null)
            {
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                {                    
                    parentNode.ChildNodes.Add(CreateObject(element));
                    //textBox1.Text += ($"{GetIndent(level)}    ->{element.Name} [{element.GetType().Name}]");
                }
            }

            foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
            {
                spatialElement = item as IIfcSpatialStructureElement;
                if (spatialElement != null)
                {
                    XTreeNode childNode = CreateObject(item);
                    parentNode.ChildNodes.Add(childNode);
                    PrintHierarchy(item, childNode);
                }
            }
        }
    }

    public static class Extensions
    {
        private static readonly char[] base64Chars = new char[]
    { '0','1','2','3','4','5','6','7','8','9'
        , 'A','B','C','D','E','F','G','H','I','J'
        , 'K','L','M','N','O','P','Q','R','S','T'
        , 'U','V','W','X','Y','Z','a','b','c','d'
        , 'e','f','g','h','i','j','k','l','m','n'
        , 'o','p','q','r','s','t','u','v','w','x'
        , 'y','z','_','$' };

        public static string ToIfcGuid(this System.Guid guid)
        {

            uint[] num = new uint[6];
            char[] str = new char[22];
            int i, n;
            byte[] b = guid.ToByteArray();

            // Creation of six 32 Bit integers from the components of the GUID structure
            num[0] = (uint)(BitConverter.ToUInt32(b, 0) / 16777216);
            num[1] = (uint)(BitConverter.ToUInt32(b, 0) % 16777216);
            num[2] = (uint)(BitConverter.ToUInt16(b, 4) * 256 + BitConverter.ToInt16(b, 6) / 256);
            num[3] = (uint)((BitConverter.ToUInt16(b, 6) % 256) * 65536 + b[8] * 256 + b[9]);
            num[4] = (uint)(b[10] * 65536 + b[11] * 256 + b[12]);
            num[5] = (uint)(b[13] * 65536 + b[14] * 256 + b[15]);

            // Conversion of the numbers into a system using a base of 64
            n = 2;
            int pos = 0;
            for (i = 0; i < 6; i++)
            {
                cv_to_64(num[i], ref str, pos, n);
                pos += n; n = 4;
            }
            return new String(str);

            void cv_to_64(uint number, ref char[] result, int start, int len)
            {
                uint act;
                int iDigit, nDigits;

                //Debug.Assert(len <= 4);
                act = number;
                nDigits = len;

                for (iDigit = 0; iDigit < nDigits; iDigit++)
                {
                    int ii = (int)(act % 64);
                    result[start + len - iDigit - 1] = base64Chars[(int)(act % 64)];
                    act = (uint)(act / 64);
                }
                //Debug.Assert(act == 0, "Logic failed, act was not null: " + act.ToString());
                return;
            }
        }
    }
}

