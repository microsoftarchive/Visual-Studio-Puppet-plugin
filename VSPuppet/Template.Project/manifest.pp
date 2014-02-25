/**********************************************************************************
*
*               Puppet manifest example
*
**********************************************************************************/

case $operatingsystem {
    centos, redhat: { $service_name = 'ntpd' }
    debian, ubuntu: { $service_name = 'ntp' }
}

package { 'ntp':
    ensure => installed,
}

service { 'ntp':
    name      => $service_name,
    ensure    => running,
    enable    => true,
    subscribe => File['ntp.conf'],
}

file { 'ntp.conf':
    path    => '/etc/ntp.conf',
    ensure  => file,
    require => Package['ntp'],
    source  => "puppet:///modules/ntp/ntp.conf",
    # This source file would be located on the puppet master at
    # /etc/puppetlabs/puppet/modules/ntp/files/ntp.conf (in Puppet Enterprise)
    # or
    # /etc/puppet/modules/ntp/files/ntp.conf (in open source Puppet)
}