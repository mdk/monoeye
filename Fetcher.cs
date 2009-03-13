/* MonoEye
 *
 * GPL license, Copyright (C) 2007 by:
 *
 * Authors:
 *      Michael Dominic K. <mdk@mdk.am>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using Gdk;
using System.Threading;

namespace MonoEye {

        public class Fetcher {

                static readonly string rssUrl = "http://api.flickr.com/services/feeds/photos_public.gne?tags={0}&format=rss_200";
                Pixbuf pixbuf;
                Thread thread;
                GLib.IdleHandler finishedHandler;
                string tag;

                public Pixbuf Pixbuf {
                        get { return pixbuf; }
                }

                public Fetcher (string tag, GLib.IdleHandler handler)
                {
                        thread = new Thread (threadStart);
                        finishedHandler = handler;
                        this.tag = tag;
                }

                public void Execute ()
                {
                        thread.Start ();
                }

                string getXml ()
                {
                        StringBuilder builder  = new StringBuilder();
		        byte[] buf = new byte[8192];

		        HttpWebRequest request  = (HttpWebRequest) WebRequest.Create (String.Format (rssUrl, tag));
		        HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
		        Stream stream = response.GetResponseStream ();

                        int count = 0;

                        do {
                                count = stream.Read (buf, 0, buf.Length);

                                if (count != 0) {
                                        string temp = Encoding.ASCII.GetString (buf, 0, count);
                                        builder.Append (temp);
                                }

                        } while (count > 0);

                        return builder.ToString ();
                }

                string [] getHits (string xml)
                {
                        ArrayList list = new ArrayList ();

                        MatchCollection matches = Regex.Matches (xml, "img src=&quot;(http://.*?_m.jpg)");

                        foreach (Match m in matches) {
                                string url = m.Groups [1].ToString ();
                                list.Add (url);
                        }

                        return (string []) list.ToArray (typeof (string));
                }

                Pixbuf getPixbuf (string url)
                {
                        HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
                        HttpWebResponse response = (HttpWebResponse) request.GetResponse ();
                        Stream stream = response.GetResponseStream ();

                        return new Gdk.Pixbuf (stream);
                }

                void threadStart ()
                {
                        string xml = getXml ();
                        string [] hits = getHits (xml);
                        if (hits.Length == 0)
                                return;

                        Random r = new Random ();
                        int p = r.Next (0, hits.Length);
                        
                        pixbuf = getPixbuf (hits [p]);

                        if (finishedHandler != null)
                                GLib.Idle.Add (finishedHandler);
                }

        }

}
