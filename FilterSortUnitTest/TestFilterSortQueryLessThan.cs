﻿namespace FilterSortUnitTest;

public class TestFilterSortQueryLessThan
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
    public void ValidateFilterDataLessThanNumber()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadEntera<18";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();
        var dataCount = _data.Where(x => x.propiedadEntera < 18).ToList();

        //assert
        Assert.AreEqual(dataCount.Count, data.Count);
    }
    [Test]
    public void ValidateFilterDataLessThanDate()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadFecha<2022-01-02";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();
        var dataCount = _data.Where(x => x.propiedadFecha < new DateTime(2022, 01, 02)).ToList();

        //assert
        Assert.AreEqual(dataCount.Count, data.Count);
    }
    [Test]
    public void ValidateFilterDataLessThanString()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadString<7";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();
        var dataCount = _data.Where(x => x.propiedadString.Length < 7).ToList();

        //assert
        Assert.AreEqual(dataCount.Count, data.Count);
    }
    [Test]
    public void ValidateFilterDataLessThanBoolean()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadBooleana<true";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();

        //assert
        Assert.AreEqual(_data.Count, data.Count);
    }
}
