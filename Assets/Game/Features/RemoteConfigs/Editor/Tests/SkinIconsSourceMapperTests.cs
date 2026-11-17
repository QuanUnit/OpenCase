using System.Collections.Generic;
using Features.RemoteConfigs.Editor.Exporters.Skins;
using Features.RemoteConfigs.Editor.Icons;
using NUnit.Framework;

namespace Features.RemoteConfigs.Editor.Tests
{
    public class SkinIconsSourceMapperTests
    {
        private SkinIconsSourceMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new SkinIconsSourceMapper();
        }

        [Test]
        public void Map_RegularSkin_IdFollowsExporterRule()
        {
            var sources = _mapper.Map(new List<SourceSkin> { CreateSkin("SSG 08", "SSG 08 | Blood in the Water", null, "url-ssg") });

            Assert.That(sources["SSG_08_Blood_in_the_Water"].ImageUrl, Is.EqualTo("url-ssg"));
        }

        [Test]
        public void Map_PhasedSkin_PhaseIdMapsToItsOwnImage()
        {
            var skins = new List<SourceSkin>
            {
                CreateSkin("Glock-18", "Glock-18 | Gamma Doppler", "Emerald", "url-emerald"),
                CreateSkin("Glock-18", "Glock-18 | Gamma Doppler", "Phase 1", "url-1"),
                CreateSkin("Glock-18", "Glock-18 | Gamma Doppler", "Phase 4", "url-4"),
            };

            var sources = _mapper.Map(skins);

            Assert.That(sources["Glock-18_Gamma_Doppler_Phase_4"].ImageUrl, Is.EqualTo("url-4"));
            Assert.That(sources["Glock-18_Gamma_Doppler_Phase_1"].ImageUrl, Is.EqualTo("url-1"));
            Assert.That(sources["Glock-18_Gamma_Doppler_Emerald"].ImageUrl, Is.EqualTo("url-emerald"));
        }

        [Test]
        public void Map_PhasedSkinWithoutPlainEntry_BaseIdUsesFirstPhase()
        {
            var skins = new List<SourceSkin>
            {
                CreateSkin("Glock-18", "Glock-18 | Gamma Doppler", "Emerald", "url-emerald"),
                CreateSkin("Glock-18", "Glock-18 | Gamma Doppler", "Phase 1", "url-1"),
            };

            var sources = _mapper.Map(skins);

            Assert.That(sources["Glock-18_Gamma_Doppler"].ImageUrl, Is.EqualTo("url-emerald"));
        }

        [Test]
        public void Map_PlainEntryAfterPhases_PlainEntryWinsForBaseId()
        {
            var skins = new List<SourceSkin>
            {
                CreateSkin("MP9", "MP9 | Doppler", "Phase 1", "url-1"),
                CreateSkin("MP9", "MP9 | Doppler", null, "url-plain"),
            };

            var sources = _mapper.Map(skins);

            Assert.That(sources["MP9_Doppler"].ImageUrl, Is.EqualTo("url-plain"));
        }

        [Test]
        public void Map_DuplicateWithoutImage_KeepsImage()
        {
            var skins = new List<SourceSkin>
            {
                CreateSkin("AK-47", "AK-47 | Redline", null, "url-redline"),
                CreateSkin("AK-47", "AK-47 | Redline", null, string.Empty),
            };

            var sources = _mapper.Map(skins);

            Assert.That(sources["AK-47_Redline"].ImageUrl, Is.EqualTo("url-redline"));
        }

        [Test]
        public void ParseSkins_ImageAndPhase_Parsed()
        {
            var json = "[{\"name\":\"Glock-18 | Gamma Doppler\",\"weapon\":{\"name\":\"Glock-18\"},\"category\":{\"name\":\"Pistols\"},\"phase\":\"Phase 4\",\"image\":\"https://cdn/x.png\"}]";

            var skins = new SkinsSourcesParser().ParseSkins(json);

            Assert.That(skins[0].phase, Is.EqualTo("Phase 4"));
            Assert.That(skins[0].image, Is.EqualTo("https://cdn/x.png"));
        }

        private SourceSkin CreateSkin(string weapon, string name, string phase, string image)
        {
            return new SourceSkin
            {
                name = name,
                weapon = new SourceNamedEntity { name = weapon },
                phase = phase,
                image = image,
            };
        }
    }
}
