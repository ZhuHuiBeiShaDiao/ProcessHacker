<?php $pagetitle = "Overview"; include "header.php"; ?>

<div class="page">
    <div class="yui-d0">
        <div class="yui-t4">
            <nav>
                <div class="logo">
                    <a href="/"><img class="flowed-block" src="img/logo_64x64.png" alt="Project Logo" width="64" height="64"></a>
                </div>

                <div class="flowed-block">
                    <h2>Process Hacker</h2>
                    <ul class="facetmenu">
                        <li class="active"><a href="/">Overview</a></li>
                        <li><a href="downloads.php">Downloads</a></li>
                        <li><a href="faq.php">FAQ</a></li>
                        <li><a href="about.php">About</a></li>
                        <li><a href="http://wj32.org/processhacker/forums/">Forum</a></li>
                    </ul>
                </div>
            </nav>

            <p class="headline main-headline">A <strong>free</strong>, powerful, multi-purpose tool that helps you <strong>monitor system resources</strong>,<br>
            <strong>debug software</strong> and <strong>detect malware</strong>.</p>

            <div class="pre-section">
                <!-- Ad Unit 3 - DO NOT CHANGE THIS CODE -->
                <div style="float: left; width: 336px; height: 280px;">
                    <?php ad_unit_3(); ?>
                </div>

                <div class="yui-b side">
                    <div class="portlet downloads">
                        <div class="version">
                            <ul>
                                <li>Process Hacker <?php echo $LATEST_PH_VERSION ?></li>
                                <li>Released <?php echo $LATEST_PH_RELEASE_DATE ?></li>
                            </ul>
                        </div>
                        <ul>
                            <li><a href="downloads.php">Download v<?php echo $LATEST_PH_VERSION." (r".$LATEST_PH_BUILD.")" ?></a></li>
                        </ul>
                        <div class="center donate">
                            <a href="http://sourceforge.net/project/project_donations.php?group_id=242527">
                                <img src="img/donate.png" alt="Donate" width="92" height="26">
                            </a>
                        </div>
                    </div>

                    <div class="portlet quick-links">
                        <h2 class="center">Quick Links</h2>
                        <ul class="involvement">
                            <li><a href="http://sourceforge.net/projects/processhacker/">SourceForge project page</a></li>
                            <li><a href="forums/viewforum.php?f=5">Ask a question</a></li>
                            <li><a href="forums/viewforum.php?f=24">Report a bug</a></li>
                            <li><a href="http://sourceforge.net/p/processhacker/code/">Browse source code</a></li>
                            <li><a href="doc/">Source code documentation</a></li>
                        </ul>
                    </div>
                </div>
            </div>

            <div class="main-section">
                <p class="section-header">Main features</p>
                <p class="headline">A detailed overview of system activity with highlighting.</p>
                <img src="img/screenshots/main_window.png" alt="Main window" width="676" height="449">

                <p class="headline">Graphs and statistics allow you quickly to track down resource hogs and runaway processes.</p>
                <img src="img/screenshots/sysinfo_trimmed_1.png" alt="System information summary" width="700" height="347">
                <p class="tip">Tip: Use Ctrl+I to view system performance information.
                Move your cursor over a graph to get a tooltip with information about the data point under your cursor.
                You can double-click the graph to see information about the process at that data point, even if the process is no longer running.</p>

                <p class="headline">Can't edit or delete a file? Discover which processes are using that file.</p>
                <img src="img/screenshots/find_handles.png" alt="Find handles" width="650" height="199">
                <p class="tip">Tip: Use Ctrl+F to search for a handle or DLL.
                If all else fails, you can right-click an entry and close the handle associated with the file. However, this
                should only be used as a last resort and can lead to data loss and corruption.</p>

                <p class="headline">See what programs have active network connections, and close them if necessary.</p>
                <img src="img/screenshots/network.png" alt="Network connections" width="641" height="296">

                <p class="headline">Get real-time information on disk access.</p>
                <img src="img/screenshots/disk_tab.png" alt="Disk tab" width="621" height="325">
                <p class="tip">Tip: This may look very similar to the Disk Activity feature in Resource Monitor, but Process Hacker has a few more features!</p>

                <p class="section-header">Advanced features</p>
                <p class="headline">View detailed stack traces with kernel-mode, WOW64 and .NET support.</p>
                <img src="img/screenshots/thread_stack.png" alt="Stack trace" width="524" height="384">
                <p class="tip">Tip: Hover your cursor over the first column (with the numbers) to view parameter and line number information when available.</p>

                <p class="headline">Go beyond services.msc: create, edit and control services.</p>
                <img src="img/screenshots/services.png" alt="Service properties" width="604" height="468">
                <p class="tip">Tip: By default, Process Hacker shows entries for drivers in addition to normal user-mode services. You can turn this off
                by checking <strong>View &gt; Hide Driver Services</strong>.</p>

                <p class="headline">And much more...</p>
                <img src="img/screenshots/menu.png" alt="Service properties" width="637" height="518">

                <p class="headline">Other additions</p>
                <p class="normal">Many of you have probably used Process Explorer in the past. Process Hacker has several advantages:</p>
                <ul class="normal">
                    <li>Process Hacker is open source and can be modified or redistributed.</li>
                    <li>Process Hacker is more customizable.</li>
                    <li>Process Hacker shows services, network connections, disk activity, and much more!</li>
                    <li>Process Hacker is better for debugging and reverse engineering.</li>
                </ul>

                <p class="headline bottom-download"><strong><a href="downloads.php?bottom=1">Download &gt;</a></strong></p>
            </div>

            <br/> <!-- this br div is a placeholder -->

            <div class="yui-g">
                <div class="yui-u first">
                    <div class="portlet">
                        <p><strong>Latest News</strong></p>
                        <div id="news_feed_div"></div>
                    </div>
                </div>
                <div class="yui-g">
                    <div class="portlet">
                        <p><strong>Forum Activity</strong></p>
                        <div id="forum_feed_div"></div>
                    </div>
                </div>
                <div class="yui-u">
                    <div id="structural-subscription-content-box"></div>
                </div>
            </div>
            <div class="yui-g">
                <div class="yui-u">
                    <div class="portlet">
                        <p><strong>SVN Activity</strong></p>
                        <div id="source_feed_div"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<?php include "footer.php"; ?>