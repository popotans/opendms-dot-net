/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    public abstract class ListBase<T> : Base
    {
        private T[] _t;
        private int _count;
        private string _rootElementName;


        public string RootElementName { get { return _rootElementName; } }
        public int Count { get { return _count; } }

        
        public ListBase(string rootElementName)
        {
            _t = new T[10];
            _rootElementName = rootElementName;
        }

        public override XmlWriter Serialize(XmlWriter xmlWriter)
        {
            if (xmlWriter.Settings.Encoding != Encoding.UTF8)
                throw new Exception("The xml writer's encoding must be UTF8");

            xmlWriter.WriteStartElement(_rootElementName);
            for (int i = 0; i < _count; i++)
                XmlSerializer.Serialize(xmlWriter, _t[i]);
            xmlWriter.WriteEndElement();

            return xmlWriter;
        }

        public override XmlReader Deserialize(XmlReader xmlReader)
        {
            bool loop = true;

            if (xmlReader.IsEmptyElement)
                return xmlReader;

            // <?xml version="1.0" encoding="utf-8"?>
            // <SearchForm>
            //   <FormProperty DataType="Text" Label="Label" PropertyName="PropName" DefaultValue="DefaultValue" />
            // </SearchForm>

            if (xmlReader.NodeType == XmlNodeType.None)
                xmlReader.MoveToContent();

            while (loop)
            {
                switch (xmlReader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (string.IsNullOrEmpty(xmlReader.LocalName))
                            xmlReader.MoveToContent();
                        if (xmlReader.LocalName == _rootElementName)
                            xmlReader.ReadStartElement(_rootElementName);
                        
                        switch (xmlReader.LocalName)
                        {
                            case "FormProperty":
                                Add(XmlSerializer.Deserialize<T>(new FormProperty(), xmlReader));
                                xmlReader.ReadStartElement();
                                break;
                            case "MetaFormProperty":
                                Add(XmlSerializer.Deserialize<T>(new MetaFormProperty(), xmlReader));
                                xmlReader.ReadStartElement();
                                break;
                            case "ListOfString":
                                xmlReader.ReadStartElement("ListOfString");
                                while(xmlReader.LocalName == "string")
                                    Add((T)XmlSerializer.SimpleDeserialize(xmlReader));
                                xmlReader.ReadEndElement();
                                return xmlReader;
                            case "MetaAsset":
                                Add(XmlSerializer.Deserialize<T>(new MetaAsset(), xmlReader));
                                //xmlReader.ReadStartElement();
                                break;
                            case "":
                                loop = false;
                                break;
                            default:
                                throw new NotSupportedException(xmlReader.LocalName + " is not supported.");
                        }

                        if (string.IsNullOrEmpty(xmlReader.LocalName))
                            xmlReader.MoveToContent();
                        if (xmlReader.LocalName == _rootElementName)
                        {
                            xmlReader.ReadEndElement(); // List
                            loop = false;
                        }
                        break;
                    case System.Xml.XmlNodeType.None:
                        loop = false;
                        break;
                    default:
                        xmlReader.MoveToContent();
                        break;
                }
            }

            return xmlReader;
        }

        public void Add(T element)
        {
            if (_count + 1 >= _t.Length)
                Expand(10);

            _t[_count++] = element;
        }

        public int IndexOf(T element)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_t[i].Equals(element))
                    return i;
            }

            return -1;
        }

        public void Remove(T element)
        {
            int index = IndexOf(element);

            if (index >= 0)
                Remove(index);
        }

        public void Remove(int index)
        {
            if (index + 1 > _count)
                    throw new IndexOutOfRangeException();

            _t[index] = default(T);
            for (int i = index; i < _count; i++)
            {
                _t[i] = _t[i + 1];
            }

            _count--;

            if (_count + 10 < _t.Length)
                Constrict(_t.Length - (_count + 10));
        }

        public T this[int index]
        {
            get
            {
                if (index + 1 > _count)
                    throw new IndexOutOfRangeException();

                return _t[index];
            }
            set
            {
                if (index + 1 > _count)
                    throw new IndexOutOfRangeException();

                _t[index] = value;
            }
        }

        private void Expand(int additionalSpace)
        {
            T[] t = new T[_t.Length + additionalSpace];
            _t.CopyTo(t, 0);
            _t = t;
        }

        private void Constrict(int removedSpace)
        {
            T[] t = new T[_t.Length - removedSpace];
            for (int i = 0; i < _count; i++)
                t[i] = _t[i];
            _t = t;
        }
    }
}
