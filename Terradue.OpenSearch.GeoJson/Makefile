
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;"
ASSEMBLY = bin/Debug/Terradue.OpenSearch.GeoJson.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Terradue.GeoJson/bin/Debug/Terradue.GeoJson.dll \
	../Terradue.OpenSearch/bin/Debug/Terradue.OpenSearch.dll
BUILD_DIR = bin/Debug

TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB_SOURCE=bin/Debug/Terradue.OpenSearch.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB=$(BUILD_DIR)/Terradue.OpenSearch.GeoJson.dll.mdb
SERVICESTACK_TEXT_DLL_SOURCE=../packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL_SOURCE=../Terradue.GeoJson/bin/Debug/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB_SOURCE=../Terradue.GeoJson/bin/Debug/Terradue.GeoJson.dll.mdb
TERRADUE_GEOJSON_DLL_MDB=$(BUILD_DIR)/Terradue.GeoJson.dll.mdb
TERRADUE_OPENSEARCH_DLL_SOURCE=../Terradue.OpenSearch/bin/Debug/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB_SOURCE=../Terradue.OpenSearch/bin/Debug/Terradue.OpenSearch.dll.mdb
TERRADUE_OPENSEARCH_DLL_MDB=$(BUILD_DIR)/Terradue.OpenSearch.dll.mdb
TERRADUE_UTIL_DLL_SOURCE=../Terradue.Util/bin/Debug/Terradue.Util.dll
TERRADUE_UTIL_DLL_MDB_SOURCE=../Terradue.Util/bin/Debug/Terradue.Util.dll.mdb
TERRADUE_UTIL_DLL_MDB=$(BUILD_DIR)/Terradue.Util.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/Terradue.OpenSearch.GeoJson.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../Terradue.GeoJson/bin/Release/Terradue.GeoJson.dll \
	../Terradue.OpenSearch/bin/Release/Terradue.OpenSearch.dll
BUILD_DIR = bin/Release

TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB=
SERVICESTACK_TEXT_DLL_SOURCE=../packages/ServiceStack.Text.3.9.71/lib/net35/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL_SOURCE=../Terradue.GeoJson/bin/Release/Terradue.GeoJson.dll
TERRADUE_GEOJSON_DLL_MDB=
TERRADUE_OPENSEARCH_DLL_SOURCE=../Terradue.OpenSearch/bin/Release/Terradue.OpenSearch.dll
TERRADUE_OPENSEARCH_DLL_MDB=
TERRADUE_UTIL_DLL_SOURCE=../Terradue.Util/bin/Release/Terradue.Util.dll
TERRADUE_UTIL_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB) \
	$(SERVICESTACK_TEXT_DLL) \
	$(TERRADUE_GEOJSON_DLL) \
	$(TERRADUE_GEOJSON_DLL_MDB) \
	$(TERRADUE_OPENSEARCH_DLL) \
	$(TERRADUE_OPENSEARCH_DLL_MDB) \
	$(TERRADUE_UTIL_DLL) \
	$(TERRADUE_UTIL_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(TERRADUE_OPENSEARCH_GEOJSON_PC)  


RESGEN=resgen2

SERVICESTACK_TEXT_DLL = $(BUILD_DIR)/ServiceStack.Text.dll
TERRADUE_GEOJSON_DLL = $(BUILD_DIR)/Terradue.GeoJson.dll
TERRADUE_OPENSEARCH_DLL = $(BUILD_DIR)/Terradue.OpenSearch.dll
TERRADUE_UTIL_DLL = $(BUILD_DIR)/Terradue.Util.dll
TERRADUE_OPENSEARCH_GEOJSON_PC = $(BUILD_DIR)/terradue.opensearch.geojson.pc

FILES = \
	Properties/AssemblyInfo.cs \
	Terradue/OpenSearch/GeoJson/Extensions/FeatureCollectionOpenSearchEngineExtension.cs \
	Terradue/OpenSearch/GeoJson/Import/ImportOptions.cs \
	Terradue/OpenSearch/GeoJson/Import/ImportUtils.cs \
	Terradue/OpenSearch/GeoJson/Import/ResultCollectionImporter.cs \
	Terradue/OpenSearch/GeoJson/Import/SyndicationFeedImporter.cs \
	Terradue/OpenSearch/GeoJson/Result/FeatureResultCollection.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Terradue.OpenSearch.GeoJson.csproj \
	terradue.opensearch.geojson.pc.in 

REFERENCES =  \
	System \
	-pkg:mono-addins \
	System.ServiceModel \
	System.Core \
	System.Xml \
	System.Net \
	System.Runtime.Serialization

DLL_REFERENCES =  \
	../ServiceStack.Text

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,SERVICESTACK_TEXT_DLL))
$(eval $(call emit-deploy-target,TERRADUE_GEOJSON_DLL))
$(eval $(call emit-deploy-target,TERRADUE_GEOJSON_DLL_MDB))
$(eval $(call emit-deploy-target,TERRADUE_OPENSEARCH_DLL))
$(eval $(call emit-deploy-target,TERRADUE_OPENSEARCH_DLL_MDB))
$(eval $(call emit-deploy-target,TERRADUE_UTIL_DLL))
$(eval $(call emit-deploy-target,TERRADUE_UTIL_DLL_MDB))
$(eval $(call emit-deploy-wrapper,TERRADUE_OPENSEARCH_GEOJSON_PC,terradue.opensearch.geojson.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

# Targets for Custom commands
DEBUG_BeforeBuild:
	(cd $(srcdir) && nuget restore $(srcdir)/..)

RELEASE_BeforeBuild:
	(cd $(srcdir) && nuget restore $(srcdir)/..)


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SERVICESTACK_TEXT_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_GEOJSON_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_UTIL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(TERRADUE_UTIL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(TERRADUE_OPENSEARCH_GEOJSON_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SERVICESTACK_TEXT_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_GEOJSON_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_GEOJSON_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_UTIL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_UTIL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(TERRADUE_OPENSEARCH_GEOJSON_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
