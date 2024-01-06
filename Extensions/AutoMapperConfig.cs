using AutoMapper;

namespace ESD_EDI_BE.Extensions
{
    public static class AutoMapperConfig<Tsource, TDestination>
    {
        private static readonly Mapper _mapper = new(new MapperConfiguration(
            cfg => cfg.CreateMap<Tsource, TDestination>().ReverseMap()
        ));

        public static TDestination Map(Tsource source)
        {
            return _mapper.Map<TDestination>(source);
        }

        public static List<TDestination> MapList(List<Tsource> source)
        {
            var list = new List<TDestination>();

            source.ForEach(x =>
            {
                list.Add(Map(x));
            });

            return list;
        }
    }
}
