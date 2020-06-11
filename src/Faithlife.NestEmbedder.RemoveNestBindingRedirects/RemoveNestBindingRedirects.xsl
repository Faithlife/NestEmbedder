<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:asm="urn:schemas-microsoft-com:asm.v1">
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="asm:assemblyBinding/asm:dependentAssembly[asm:assemblyIdentity[@name='Elasticsearch.Net' or @name='Nest']]/asm:bindingRedirect"/>
</xsl:stylesheet>
