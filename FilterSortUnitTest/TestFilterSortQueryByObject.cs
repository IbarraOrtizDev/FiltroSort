using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterSortUnitTest
{
    public class TestFilterSortQueryByObject
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
        public void ValidateFilterDataByObject()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadObjeto.propiedadString==Texto1-0";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadObjeto?.propiedadString == "Texto1-0").Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }

        [Test]
        public void ValidateFilterDataByObjectNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadObjeto.propiedadEntera==1,propiedadString==Texto3";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadObjeto?.propiedadEntera == 1 && x.propiedadString=="Texto3").Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }

        [Test]
        public void ValidateFilterDataByObjectListString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadObjeto.propiedadListaString>0";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadObjeto?.propiedadListaString?.Count()>0).Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }

        [Test]
        public void ValidateFilterDataByObjectContainsListString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadObjeto.propiedadListaString@=Texto2";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadObjeto?.propiedadListaString?.Contains("Texto2") ??false).Count();
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
    }
}
