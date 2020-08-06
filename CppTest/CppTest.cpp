// CppTest.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <sstream>
#include <iostream>
#include <iomanip>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <regex>
#include <iterator>
#include <winsock2.h>
#include <iphlpapi.h>
#include <windows.h>
#include <windns.h>
#include <ws2tcpip.h>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "Dnsapi.lib")

void func1();

std::wstring GetLastErrorMsg();
BOOL CheckInternetConnectivity();
void threadFunc(HANDLE timer);

int main() {
    func1();
}

void func1() {
    HANDLE hWaitableTimer = CreateWaitableTimer(NULL, FALSE, NULL);
    LARGE_INTEGER dueTime;
    dueTime.QuadPart = 0;
    BOOL result = SetWaitableTimer(hWaitableTimer, &dueTime, 5000, NULL, NULL, FALSE);
    if (!result) {
        std::cerr << "Failed to set waitable timer" << std::endl;
    }
    bool error = false;

    HANDLE hAddrChange = NULL;
    OVERLAPPED oAddrChange;
    ZeroMemory(&oAddrChange, sizeof(oAddrChange));
    oAddrChange.hEvent = WSACreateEvent();
    DWORD dwResult = NotifyAddrChange(&hAddrChange, &oAddrChange);
    if (dwResult != ERROR_IO_PENDING) {
        wprintf(L"Failed to register address change event: %d, %s\n", dwResult, GetLastErrorMsg().c_str());
        error = true;
    }

    HANDLE hRouteChange = NULL;
    OVERLAPPED oRouteChange;
    ZeroMemory(&oRouteChange, sizeof(oRouteChange));
    oRouteChange.hEvent = WSACreateEvent();
    dwResult = NotifyRouteChange(&hRouteChange, &oRouteChange);
    if (dwResult != ERROR_IO_PENDING) {
        wprintf(L"Failed to register route change event: %d, %s\n", dwResult, GetLastErrorMsg().c_str());
        error = true;
    }

    int i = 0;
    while (!error) {
        i++;
        HANDLE handles[] = { hWaitableTimer, oAddrChange.hEvent, oRouteChange.hEvent };
        BOOL connectivity;
        std::cout << i << " Starting to wait" << std::endl;
        DWORD waitResult = WaitForMultipleObjectsEx(3, handles, FALSE, INFINITE, TRUE);

        switch (waitResult) {
        case WAIT_IO_COMPLETION:
            std::cout << i << " An IO operation is completed." << std::endl;
            break;

        case WAIT_OBJECT_0:
            std::cout << i << " internet interval reached, checking for internet connectivity" << std::endl;
            connectivity = CheckInternetConnectivity();
            break;

        case WAIT_OBJECT_0 + 1:
            std::cout << i << " address change event: " << std::endl;
            dwResult = NotifyAddrChange(&hAddrChange, &oAddrChange);
            if (dwResult != ERROR_IO_PENDING) {
                wprintf(L"Failed to register address change event: %d, %s\n", dwResult, GetLastErrorMsg().c_str());
                error = true;
            }
            break;

        case WAIT_OBJECT_0 + 2:
            std::cout << i << " route change event: " << std::endl;
            dwResult = NotifyRouteChange(&hRouteChange, &oRouteChange);
            if (dwResult != ERROR_IO_PENDING) {
                wprintf(L"Failed to register route change event: %d, %s\n", dwResult, GetLastErrorMsg().c_str());
                error = true;
            }
            break;

        default:
            std::cerr << i << " wait failed." << std::endl;
            error = true;
            break;
        }

        std::cout << std::endl;
    }

    if (error) {
        std::cout << "error exit" << std::endl;
    }
}


DNS_QUERY_COMPLETION_ROUTINE DnsQueryCompletionRoutine;
void __stdcall DnsQueryCompletionRoutine(PVOID pQueryContext, PDNS_QUERY_RESULT pQueryResults)
{
    std::cout << "GOT DNS response." << std::endl;
    if (pQueryResults->QueryStatus == ERROR_SUCCESS) {
        std::cout << "DNS successful." << std::endl;
    }
    else {
        std::cout << "DNS error: " << pQueryResults->QueryStatus << std::endl;
    }

    auto firstRecords = pQueryResults->pQueryRecords;
    if (firstRecords != NULL && firstRecords->wType == DNS_TYPE_A) {
        IP4_ADDRESS address = firstRecords->Data.A.IpAddress;
        char str[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &address, str, INET_ADDRSTRLEN);
        std::cout << "RESOLVED IP: " << str << std::endl;
    }

    DnsRecordListFree(pQueryResults->pQueryRecords, DnsFreeRecordList);
}

BOOL CheckInternetConnectivity() {

    struct sockaddr_in sockAddr;
    sockAddr.sin_family = AF_INET;
    sockAddr.sin_port = htons(53);
    InetPtonW(AF_INET, L"34.250.116.108", &sockAddr.sin_addr);

    DNS_ADDR dnsAddr;
    ZeroMemory(&dnsAddr, sizeof(dnsAddr));
    memcpy_s(&dnsAddr, sizeof(dnsAddr), &sockAddr, sizeof(sockAddr));

    DNS_ADDR_ARRAY servers;
    ZeroMemory(&servers, sizeof(servers));
    servers.MaxCount = sizeof(DNS_ADDR_ARRAY);
    servers.AddrCount = 1;
    servers.Family = AF_INET;
    servers.AddrArray[0] = dnsAddr;

    DNS_QUERY_REQUEST request;
    ZeroMemory(&request, sizeof(request));
    request.Version = DNS_QUERY_REQUEST_VERSION1;
    request.QueryName = L"-.-";
    request.QueryType = DNS_TYPE_A;
    request.QueryOptions = DNS_QUERY_BYPASS_CACHE;
    request.pDnsServerList = &servers;
    request.pQueryCompletionCallback = DnsQueryCompletionRoutine;
    request.pQueryContext = NULL;

    PDNS_QUERY_RESULT pResult = (PDNS_QUERY_RESULT)malloc(sizeof(DNS_QUERY_RESULT));
    ZeroMemory(pResult, sizeof(DNS_QUERY_RESULT));
    pResult->Version = DNS_QUERY_REQUEST_VERSION1;

    PDNS_QUERY_CANCEL pCancel = (PDNS_QUERY_CANCEL)malloc(sizeof(DNS_QUERY_CANCEL));
    ZeroMemory(pCancel, sizeof(DNS_QUERY_CANCEL));

    DNS_STATUS dnsStatus = DnsQueryEx(&request, pResult, pCancel);
    if (dnsStatus == ERROR_INVALID_PARAMETER) {
        std::cout << "DNS query: invalid parameter" << std::endl;
    }
    else if (dnsStatus == DNS_REQUEST_PENDING) {
        std::cout << "DNS query: out and pending" << std::endl;
    }
    else {
        std::cout << "DNS query: error code " << dnsStatus << std::endl;
    }

    return TRUE;
}

std::wstring GetLastErrorMsg() {
    DWORD dw = GetLastError();
    LPVOID lpMsgBuf;

    FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER |
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        dw,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR)&lpMsgBuf,
        0, NULL);

    auto errMsg = std::wstring((LPTSTR)lpMsgBuf);

    return errMsg;
}




void func2() {
    for (std::string uriString; std::getline(std::cin, uriString); uriString != "") {

        std::string scheme;
        std::string host;
        int port = 0;
        std::string path;

        bool ipv4;
        bool ipv6;

        // get scheme
        std::regex schemeRegex(R"d(^(\w+)://)d");
        auto begin = std::sregex_iterator(uriString.begin(), uriString.end(), schemeRegex);
        auto end = std::sregex_iterator();
        if (begin != end) {
            auto match = *begin;
            scheme = match[1].str();

            uriString = std::regex_replace(uriString, schemeRegex, "");
        }

        // get path
        std::regex pathRegex(R"d(/.*$)d");
        begin = std::sregex_iterator(uriString.begin(), uriString.end(), pathRegex);
        if (begin != end) {
            auto match = *begin;
            path = match.str();
            uriString = std::regex_replace(uriString, pathRegex, "");
        }

        // get port
        std::regex portRegex(R"d(:(\d+)$)d");
        begin = std::sregex_iterator(uriString.begin(), uriString.end(), portRegex);
        if (begin != end) {
            auto match = *begin;
            auto portStr = match[1].str();
            port = atoi(portStr.c_str());
            uriString = std::regex_replace(uriString, portRegex, "");
        }

        host = uriString;

        std::regex ipv4Regex(R"d(^(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}))d");
        ipv4 = std::regex_match(host, ipv4Regex);

        // So very simple and not at all working IPV6 check.
        ipv6 = host.find(':') != std::string::npos;

        std::cout << std::endl;
        std::cout << "scheme: " << scheme << std::endl;
        std::cout << "host: " << host << std::endl;
        std::cout << "port: " << port << std::endl;
        std::cout << "path: " << path << std::endl;
        std::cout << "is ipv4: " << ipv4 << std::endl;
        std::cout << "is ipv6: " << ipv6 << std::endl;
        std::cout << std::endl;
    }
}