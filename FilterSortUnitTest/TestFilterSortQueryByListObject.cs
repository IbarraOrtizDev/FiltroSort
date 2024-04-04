using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterSortUnitTest
{
    public class TestFilterSortQueryByListObject
    {
        List<DataDTO> _data;
        [SetUp]
        public void Setup()
        {
            ServiceData serviceData = new ServiceData();
            _data = serviceData.GetData();
        }

        [Test]
        public void ValidateContainData()
        {
            Assert.AreNotEqual(0, _data.Count);
        }


        [Test]
        public void ValidateFilterDataByLisObjectString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadListaObjetos.propiedadString==Texto1-0";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadListaObjetos != null && x.propiedadListaObjetos.Any(x=>x.propiedadString== "Texto1-0")).Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
        [Test]
        public void ValidateFilterDataByListObjectCount()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadListaObjetos.propiedadListaString>0";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadListaObjetos != null && x.propiedadListaObjetos.Any(x => x.propiedadListaString?.Count() > 0)).Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }

        [Test]
        public void ValidateFilterDataByListObjectContains()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadListaObjetos.propiedadListaString@=Texto1";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadListaObjetos != null && x.propiedadListaObjetos.Any(x => x.propiedadListaString.Contains("Texto1"))).Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
    }
}
