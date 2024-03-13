using FilterSort.Models;
using FiltroSort;
using FiltroSortApi.DTOs;
using FiltroSortApi.Services;

namespace FilterSortUnitTest
{
    public class TestFilterSortQueryNotEquals
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
        public void ValidateFilterDataNotEqualsString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString!=Texto18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString != "Texto18").ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataNotEqualsNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera!=18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadEntera != 18).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataNotEqualsDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha!=2022-01-01";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadFecha != new DateTime(2022, 01, 01)).ToList();

            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataNotEqualsBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBooleana!=true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadBooleana != true).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
    }
}
