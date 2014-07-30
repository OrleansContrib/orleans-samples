//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class CityCoords
    {
        public string CityName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public CityCoords(string name, double lat, double lon)
        {
            this.CityName = name;
            this.Latitude = lat;
            this.Longitude = lon;
        }
    }

    public class CityCoordinates
    {
        public CityCoords[] Coordinates { get; set; }

        public CityCoordinates()
        {
            this.Coordinates = new CityCoords[]
            {
                new CityCoords("Buenos Aires",-34.61315,-58.37723),
                new CityCoords("Dhaka",23.7104,90.40744),
                new CityCoords("São Paulo",-23.5475,-46.63611),
                new CityCoords("Rio de Janeiro",-22.90278,-43.2075),
                new CityCoords("Kinshasa",-4.32758,15.31357),
                new CityCoords("Zhumadian",32.97944,114.02944),
                new CityCoords("Tai’an",36.18528,117.12),
                new CityCoords("Shanghai",31.22222,121.45806),
                new CityCoords("Nanchong",30.79508,106.08474),
                new CityCoords("Beijing",39.9075,116.39723),
                new CityCoords("Bogotá",4.60971,-74.08175),
                new CityCoords("Cairo",30.06263,31.24967),
                new CityCoords("City of London",51.51279,-0.09184),
                new CityCoords("London",51.50853,-0.12574),
                new CityCoords("Hong Kong",22.28552,114.15769),
                new CityCoords("Jakarta",-6.21462,106.84513),
                new CityCoords("Delhi",28.65381,77.22897),
                new CityCoords("Mumbai",19.07283,72.88261),
                new CityCoords("Bangalore",12.97194,77.59369),
                new CityCoords("Baghdad",33.34058,44.40088),
                new CityCoords("Tehrān",35.69439,51.42151),
                new CityCoords("Tokyo",35.6895,139.69171),
                new CityCoords("Seoul",37.566,126.9784),
                new CityCoords("Mexico City",19.42847,-99.12766),
                new CityCoords("Lagos",6.45306,3.39583),
                new CityCoords("Lima",-12.04318,-77.02824),
                new CityCoords("Manila",14.6042,120.9822),
                new CityCoords("Lahore",31.54972,74.34361),
                new CityCoords("Karachi",24.9056,67.0822),
                new CityCoords("Saint Petersburg",59.93863,30.31413),
                new CityCoords("Moscow",55.75222,37.61556),
                new CityCoords("Bangkok",13.75398,100.50144),
                new CityCoords("İstanbul",41.01384,28.94966),
                new CityCoords("Taipei",25.04776,121.53185),
                new CityCoords("New York City",40.71427,-74.00597)
            };
        }

        public CityCoords RandomCity()
        {
            var rand = new Random();
            return Coordinates[rand.Next(Coordinates.Length)];
        }
    }
}
