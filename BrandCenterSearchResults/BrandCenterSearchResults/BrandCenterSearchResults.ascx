<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BrandCenterSearchResults.ascx.cs" Inherits="BrandCenterSearchResults.BrandCenterSearchResults" %>
<%@ Import Namespace="BrandCenterSearchResults" %>

<p>
    Your search for "<b><u><%=SearchTerm %></u></b>" returned <i><b><%=this.SearchResults.Count()%></b> result(s)</i>
</p>

<style>
.umbSearchHighlight { font-weight:bold;}
</style>

<asp:Repeater ID="SearchResultListing" runat="server">

    <HeaderTemplate>
        <ul>
    </HeaderTemplate>
    <ItemTemplate>
        <li>
            <a href='<%# ((Examine.SearchResult)Container.DataItem).FullUrl() %>'>
            <%#GetTitle(Container)%>
            </a>
            <p><%#GetSearchResultHighlight(Container)%></p>
        </li>
    </ItemTemplate>
    <FooterTemplate>
        </ul>
    </FooterTemplate>

</asp:Repeater>